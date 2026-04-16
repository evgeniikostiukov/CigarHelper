using CigarHelper.Api.Options;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Хранение изображений в MinIO (S3-совместимом объектном хранилище).
/// Используется при <c>ImageStorage:Provider = "Minio"</c>.
/// При старте гарантирует существование бакета (создаёт при необходимости).
/// </summary>
public sealed class MinioImageStorageProvider : IImageStorageProvider, IAsyncDisposable
{
    private readonly IMinioClient _client;
    private readonly string _bucketName;
    private readonly ILogger<MinioImageStorageProvider> _logger;

    public bool StoresExternally => true;

    public MinioImageStorageProvider(MinioOptions opts, ILogger<MinioImageStorageProvider> logger)
    {
        _bucketName = opts.BucketName;
        _logger = logger;

        _client = new MinioClient()
            .WithEndpoint(opts.Endpoint)
            .WithCredentials(opts.AccessKey, opts.SecretKey)
            .WithSSL(opts.UseSsl)
            .Build();
    }

    /// <summary>
    /// Инициализирует бакет (создаёт, если не существует).
    /// Вызывается при регистрации провайдера в DI через <c>IHostedService</c> или явно из Program.cs.
    /// </summary>
    public async Task EnsureBucketExistsAsync(CancellationToken ct = default)
    {
        try
        {
            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucketName), ct);

            if (!exists)
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucketName), ct);
                _logger.LogInformation("MinIO: создан бакет {Bucket}", _bucketName);
            }
        }
        catch (Exception ex)
        {
            // Не блокируем остальной API (список сигар и т.д.): изображения просто не отдадутся, пока MinIO недоступен.
            _logger.LogWarning(ex, "MinIO: не удалось проверить или создать бакет {Bucket}", _bucketName);
        }
    }

    public async Task<string?> SaveAsync(byte[] data, string suggestedFileName, CancellationToken ct = default)
    {
        var objectName = $"{Guid.NewGuid():N}_{SanitizeFileName(suggestedFileName)}";
        using var stream = new MemoryStream(data);
        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(data.Length)
                .WithContentType(DetectContentType(suggestedFileName)),
            ct);

        return objectName;
    }

    public async Task<byte[]?> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();

        try
        {
            await _client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(storagePath)
                    .WithCallbackStream((stream, token) => stream.CopyToAsync(ms, token)),
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MinIO: не удалось прочитать объект {Object}", storagePath);
            return null;
        }

        return ms.ToArray();
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        try
        {
            await _client.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(storagePath),
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MinIO: не удалось удалить объект {Object}", storagePath);
        }
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
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
            ".avif" => "image/avif",
            _ => "application/octet-stream"
        };
}
