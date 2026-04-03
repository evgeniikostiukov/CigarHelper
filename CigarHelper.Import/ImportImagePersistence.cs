using System.Security.Cryptography;
using System.Text;
using CigarHelper.Api.Options;
using CigarHelper.Api.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CigarHelper.Import;

/// <summary>
/// Сохраняет изображения импорта через <see cref="IImageStorageProvider"/> (тот же MinIO/LocalFile, что и API).
/// Ключи объектов детерминированы от нормализованного URL картинки (SHA256), чтобы повторный импорт не скачивал и не перезаливал те же файлы.
/// </summary>
public sealed class ImportImagePersistence
{
    private readonly IImageStorageProvider _storage;
    private readonly ImageStorageOptions _storageOptions;
    private readonly ILogger<ImportImagePersistence> _logger;

    public ImportImagePersistence(
        IImageStorageProvider storage,
        IOptions<ImageStorageOptions> storageOptions,
        ILogger<ImportImagePersistence> logger)
    {
        _storage = storage;
        _storageOptions = storageOptions.Value;
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

        var mainMeta = await _storage.TryDescribeAsync(mainKey, cancellationToken);
        if (mainMeta is null)
            return null;

        var thumbMeta = await _storage.TryDescribeAsync(thumbKey, cancellationToken);
        if (thumbMeta is null)
            return null;

        return new ExistingImportImageInfo(
            mainKey,
            thumbKey,
            mainMeta.Size,
            mainMeta.ContentType ?? DetectContentType(fileName));
    }

    public async Task<(string StoragePath, string? ThumbnailPath)> SaveImageAsync(
        byte[] data,
        string fileName,
        string sourceImageUrl,
        CancellationToken cancellationToken = default)
    {
        var thumbW = _storageOptions.ThumbnailMaxWidth > 0 ? _storageOptions.ThumbnailMaxWidth : 320;
        var thumbH = _storageOptions.ThumbnailMaxHeight > 0 ? _storageOptions.ThumbnailMaxHeight : 320;

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
        await _storage.PutAtKeyAsync(data, mainKey, DetectContentType(fileName), cancellationToken);

        string? thumbKeyOut = null;
        if (thumbBytes is { Length: > 0 })
        {
            await _storage.PutAtKeyAsync(thumbBytes, thumbKey, "image/webp", cancellationToken);
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
