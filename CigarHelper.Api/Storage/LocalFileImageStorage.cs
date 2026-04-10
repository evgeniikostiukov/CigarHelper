using Microsoft.Extensions.Logging;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Хранение изображений на локальном диске.
/// Используется при <c>ImageStorage:Provider = "LocalFile"</c>.
/// При горизонтальном масштабировании требует общего сетевого тома (NFS/SMB)
/// или замены на объектное хранилище (S3/MinIO).
/// </summary>
public sealed class LocalFileImageStorage : IImageStorageProvider
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileImageStorage> _logger;

    public bool StoresExternally => true;

    public LocalFileImageStorage(string basePath, ILogger<LocalFileImageStorage> logger)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
        _logger = logger;
    }

    public async Task<string?> SaveAsync(byte[] data, string suggestedFileName, CancellationToken ct = default)
    {
        var safeFileName = $"{Guid.NewGuid():N}_{SanitizeFileName(suggestedFileName)}";
        var fullPath = Path.Combine(_basePath, safeFileName);
        await File.WriteAllBytesAsync(fullPath, data, ct);
        return safeFileName;
    }

    public async Task<byte[]?> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Image file not found: {Path}", fullPath);
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath, ct);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, storagePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image file at {Path}", storagePath);
        }

        return Task.CompletedTask;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var result = string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        return result.Length > 100 ? result[..100] : result;
    }
}
