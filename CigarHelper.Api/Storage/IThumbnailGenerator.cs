using CigarHelper.Api.Options;

namespace CigarHelper.Api.Storage;

/// <summary>Генерирует миниатюру из бинарных данных изображения.</summary>
public interface IThumbnailGenerator
{
    /// <summary>
    /// Создаёт миниатюру по политике <see cref="ImageStorageOptions.Compression"/> и лимитам
    /// <see cref="ImageStorageOptions.ThumbnailMaxWidth"/> / <see cref="ImageStorageOptions.ThumbnailMaxHeight"/>.
    /// Возвращает WebP-байты.
    /// </summary>
    Task<byte[]> GenerateAsync(
        byte[] sourceData,
        ImageStorageOptions imageStorageOptions,
        CancellationToken ct = default);
}
