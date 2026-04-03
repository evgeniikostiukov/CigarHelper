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

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public async Task PutAtKeyAsync(byte[] data, string storagePath, string contentType, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(storagePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        await File.WriteAllBytesAsync(fullPath, data, ct);
    }

    public Task<ImageStorageObjectInfo?> TryDescribeAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(storagePath);
        if (!File.Exists(fullPath))
            return Task.FromResult<ImageStorageObjectInfo?>(null);

        try
        {
            var info = new FileInfo(fullPath);
            return Task.FromResult<ImageStorageObjectInfo?>(new ImageStorageObjectInfo(info.Length, null));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to stat file at {Path}", storagePath);
            return Task.FromResult<ImageStorageObjectInfo?>(null);
        }
    }

    private string ResolvePath(string storagePath)
    {
        var rel = storagePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_basePath, rel);
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
