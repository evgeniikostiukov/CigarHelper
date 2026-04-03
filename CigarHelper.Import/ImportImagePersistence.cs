using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CigarHelper.Import;

/// <summary>
/// Сохраняет изображения импорта в MinIO или на диск — та же политика, что и у API (<c>ImageStorage</c>).
/// </summary>
public sealed class ImportImagePersistence : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImportImagePersistence> _logger;
    private IMinioClient? _minio;
    private string? _minioBucket;
    private string? _localBasePath;
    private readonly object _gate = new();

    public ImportImagePersistence(IConfiguration configuration, ILogger<ImportImagePersistence> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(string StoragePath, string? ThumbnailPath)> SaveImageAsync(
        byte[] data,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var provider = (_configuration["ImageStorage:Provider"] ?? "Minio").Trim();
        var thumbW = int.TryParse(_configuration["ImageStorage:ThumbnailMaxWidth"], out var tw) ? tw : 320;
        var thumbH = int.TryParse(_configuration["ImageStorage:ThumbnailMaxHeight"], out var th) ? th : 320;

        byte[]? thumbBytes = null;
        try
        {
            thumbBytes = await GenerateWebpThumbnailAsync(data, thumbW, thumbH, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Импорт: не удалось сгенерировать миниатюру для {File}", fileName);
        }

        if (provider.Equals("LocalFile", StringComparison.OrdinalIgnoreCase))
        {
            var main = await SaveToLocalAsync(data, fileName, cancellationToken);
            string? thumbPath = null;
            if (thumbBytes is { Length: > 0 })
                thumbPath = await SaveToLocalAsync(thumbBytes, "thumb_" + fileName + ".webp", cancellationToken);
            return (main, thumbPath);
        }

        await EnsureMinioReadyAsync(cancellationToken);
        var mainKey = await PutMinioObjectAsync(data, fileName, cancellationToken);
        string? thumbKey = null;
        if (thumbBytes is { Length: > 0 })
            thumbKey = await PutMinioObjectAsync(thumbBytes, "thumb_" + fileName + ".webp", cancellationToken);
        return (mainKey, thumbKey);
    }

    private async Task EnsureMinioReadyAsync(CancellationToken ct)
    {
        lock (_gate)
        {
            if (_minio != null)
                return;
        }

        var endpoint = _configuration["ImageStorage:Minio:Endpoint"] ?? "localhost:9000";
        var bucket = _configuration["ImageStorage:Minio:BucketName"] ?? "cigar-images";
        var access = _configuration["ImageStorage:Minio:AccessKey"] ?? "";
        var secret = _configuration["ImageStorage:Minio:SecretKey"] ?? "";
        var useSsl = bool.TryParse(_configuration["ImageStorage:Minio:UseSsl"], out var ssl) && ssl;

        var client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(access, secret)
            .WithSSL(useSsl)
            .Build();

        var exists = await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket), ct);
        if (!exists)
        {
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);
            _logger.LogInformation("Импорт MinIO: создан бакет {Bucket}", bucket);
        }

        lock (_gate)
        {
            _minio = client;
            _minioBucket = bucket;
        }
    }

    private async Task<string> PutMinioObjectAsync(byte[] data, string suggestedName, CancellationToken ct)
    {
        if (_minio is null || _minioBucket is null)
            throw new InvalidOperationException("MinIO не инициализирован.");

        var objectName = $"{Guid.NewGuid():N}_{SanitizeFileName(suggestedName)}";
        using var stream = new MemoryStream(data);
        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_minioBucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(data.Length)
                .WithContentType(DetectContentType(suggestedName)),
            ct);
        return objectName;
    }

    private async Task<string> SaveToLocalAsync(byte[] data, string suggestedName, CancellationToken ct)
    {
        var relativeRoot = _configuration["ImageStorage:LocalPath"] ?? "uploads/images";
        lock (_gate)
        {
            if (_localBasePath == null)
            {
                _localBasePath = Path.IsPathRooted(relativeRoot)
                    ? relativeRoot
                    : Path.Combine(AppContext.BaseDirectory, relativeRoot);
                Directory.CreateDirectory(_localBasePath);
            }
        }

        var safe = $"{Guid.NewGuid():N}_{SanitizeFileName(suggestedName)}";
        var full = Path.Combine(_localBasePath!, safe);
        await File.WriteAllBytesAsync(full, data, ct);
        return safe;
    }

    private static async Task<byte[]> GenerateWebpThumbnailAsync(
        byte[] sourceData,
        int maxWidth,
        int maxHeight,
        CancellationToken ct)
    {
        using var image = Image.Load(sourceData);
        var (origW, origH) = (image.Width, image.Height);
        if (origW > maxWidth || origH > maxHeight)
        {
            var ratioW = (double)maxWidth / origW;
            var ratioH = (double)maxHeight / origH;
            var ratio = Math.Min(ratioW, ratioH);
            var newW = Math.Max(1, (int)(origW * ratio));
            var newH = Math.Max(1, (int)(origH * ratio));
            image.Mutate(x => x.Resize(newW, newH));
        }

        using var ms = new MemoryStream();
        var encoder = new WebpEncoder { Quality = 80 };
        await image.SaveAsync(ms, encoder, ct);
        return ms.ToArray();
    }

    public ValueTask DisposeAsync()
    {
        _minio?.Dispose();
        return ValueTask.CompletedTask;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var result = string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        return result.Length > 100 ? result[..100] : result;
    }

    private static string DetectContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
}
