using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;

namespace CigarHelper.Api.Services;

public interface IImageService
{
    /// <summary>
    /// Сохраняет новое изображение (бинарные данные + метаданные) через текущий storage provider,
    /// генерирует миниатюру и записывает сущность в БД.
    /// </summary>
    Task<CigarImage> SaveImageAsync(
        byte[]? imageData,
        string? contentType,
        string? fileName,
        string? description,
        bool isMain,
        int? cigarBaseId,
        int? userCigarId,
        CancellationToken ct = default);

    /// <summary>
    /// Обновляет бинарные данные изображения (если переданы) и регенерирует миниатюру.
    /// </summary>
    Task UpdateImageDataAsync(CigarImage image, byte[] newData, string? contentType, CancellationToken ct = default);

    /// <summary>Удаляет изображение из БД и из внешнего хранилища (если используется).</summary>
    Task DeleteImageAsync(CigarImage image, CancellationToken ct = default);

    /// <summary>Возвращает бинарные данные полного изображения.</summary>
    Task<(byte[]? Data, string ContentType)> GetImageDataAsync(CigarImage image, CancellationToken ct = default);

    /// <summary>Возвращает бинарные данные миниатюры (WebP).</summary>
    Task<byte[]?> GetThumbnailDataAsync(CigarImage image, CancellationToken ct = default);
}
