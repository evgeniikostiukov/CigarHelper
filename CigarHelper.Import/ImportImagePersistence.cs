using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CigarHelper.Import;

/// <summary>
/// Сохраняет изображения импорта в MinIO или на диск — та же политика, что и у API (<c>ImageStorage</c>).
/// Ключи объектов детерминированы от нормализованного URL картинки (SHA256), чтобы повторный импорт не скачивал и не перезаливал те же файлы.
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

    /// <summary>
    /// Если в хранилище уже лежат оригинал и миниатюра для этого URL источника, возвращает пути без скачивания.
    /// </summary>
    public async Task<ExistingImportImageInfo?> TryGetExistingBySourceImageUrlAsync(
        string sourceImageUrl,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceImageUrl))
            return null;

        var (mainKey, thumbKey) = GetDeterministicKeys(sourceImageUrl, fileName);
        var provider = (_configuration["ImageStorage:Provider"] ?? "Minio").Trim();

        if (provider.Equals("LocalFile", StringComparison.OrdinalIgnoreCase))
        {
            EnsureLocalBase();
            var mainFull = LocalFullPath(mainKey);
            var thumbFull = LocalFullPath(thumbKey);
            if (File.Exists(mainFull) && File.Exists(thumbFull))
            {
                var len = new FileInfo(mainFull).Length;
                return new ExistingImportImageInfo(
                    mainKey,
                    thumbKey,
                    len,
                    DetectContentType(fileName));
            }

            return null;
        }

        await EnsureMinioReadyAsync(cancellationToken);
        if (_minio is null || _minioBucket is null)
            return null;

        try
        {
            var mainStat = await _minio.StatObjectAsync(
                new StatObjectArgs().WithBucket(_minioBucket).WithObject(mainKey),
                cancellationToken);
            await _minio.StatObjectAsync(
                new StatObjectArgs().WithBucket(_minioBucket).WithObject(thumbKey),
                cancellationToken);
            return new ExistingImportImageInfo(
                mainKey,
                thumbKey,
                (long)mainStat.Size,
                mainStat.ContentType ?? DetectContentType(fileName));
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task<(string StoragePath, string? ThumbnailPath)> SaveImageAsync(
        byte[] data,
        string fileName,
        string sourceImageUrl,
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

        var (mainKey, thumbKey) = GetDeterministicKeys(sourceImageUrl, fileName);

        if (provider.Equals("LocalFile", StringComparison.OrdinalIgnoreCase))
        {
            EnsureLocalBase();
            await WriteLocalObjectAsync(mainKey, data, cancellationToken);
            string? thumbPath = null;
            if (thumbBytes is { Length: > 0 })
            {
                await WriteLocalObjectAsync(thumbKey, thumbBytes, cancellationToken);
                thumbPath = thumbKey;
            }

            return (mainKey, thumbPath);
        }

        await EnsureMinioReadyAsync(cancellationToken);
        await PutMinioObjectAtKeyAsync(mainKey, data, DetectContentType(fileName), cancellationToken);
        string? thumbKeyOut = null;
        if (thumbBytes is { Length: > 0 })
        {
            await PutMinioObjectAtKeyAsync(thumbKey, thumbBytes, "image/webp", cancellationToken);
            thumbKeyOut = thumbKey;
        }

        return (mainKey, thumbKeyOut);
    }

    private static string NormalizeImageUrl(string url)
    {
        var t = url.Trim();
        if (!Uri.TryCreate(t, UriKind.Absolute, out var uri))
            return t;
        return uri.AbsoluteUri;
    }

    private static (string MainKey, string ThumbKey) GetDeterministicKeys(string sourceImageUrl, string fileName)
    {
        var norm = NormalizeImageUrl(sourceImageUrl);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(norm))).ToLowerInvariant();
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext))
            ext = ".jpg";
        var mainKey = $"import/{hash}{ext}";
        var thumbKey = $"import/{hash}_thumb.webp";
        return (mainKey, thumbKey);
    }

    private string LocalFullPath(string storageRelativeKey)
    {
        if (_localBasePath == null)
            throw new InvalidOperationException("Local base не инициализирован.");
        var rel = storageRelativeKey.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_localBasePath, rel);
    }

    private void EnsureLocalBase()
    {
        lock (_gate)
        {
            if (_localBasePath != null)
                return;
        }

        var relativeRoot = _configuration["ImageStorage:LocalPath"] ?? "uploads/images";
        var basePath = Path.IsPathRooted(relativeRoot)
            ? relativeRoot
            : Path.Combine(AppContext.BaseDirectory, relativeRoot);
        Directory.CreateDirectory(basePath);
        Directory.CreateDirectory(Path.Combine(basePath, "import"));
        lock (_gate)
        {
            _localBasePath ??= basePath;
        }
    }

    private async Task WriteLocalObjectAsync(string relativeKey, byte[] data, CancellationToken ct)
    {
        EnsureLocalBase();
        var full = LocalFullPath(relativeKey);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        await File.WriteAllBytesAsync(full, data, ct);
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

    private async Task PutMinioObjectAtKeyAsync(string objectName, byte[] data, string contentType, CancellationToken ct)
    {
        if (_minio is null || _minioBucket is null)
            throw new InvalidOperationException("MinIO не инициализирован.");

        using var stream = new MemoryStream(data);
        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_minioBucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(data.Length)
                .WithContentType(contentType),
            ct);
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

    internal static string DetectContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
}

/// <summary>Уже сохранённые в хранилище пары оригинал + миниатюра для URL картинки из CSV.</summary>
public sealed record ExistingImportImageInfo(
    string StoragePath,
    string ThumbnailPath,
    long? FileSize,
    string ContentType);
