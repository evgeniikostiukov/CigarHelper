using CigarHelper.Api.Options;
using CigarHelper.Data.Models;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Запись оригинала и миниатюры во внешнее хранилище — та же схема ключей, что при загрузке из UI (<see cref="CigarHelper.Api.Services.ImageService"/>).
/// </summary>
public static class CigarImageStorageWriter
{
    public static async Task WriteOriginalAndThumbnailAsync(
        CigarImage image,
        byte[] data,
        IImageStorageProvider storage,
        IThumbnailGenerator thumbnails,
        ImageStorageOptions options,
        CancellationToken ct = default)
    {
        image.FileSize = data.Length;

        image.StoragePath = await storage.SaveAsync(data, image.FileName ?? "image", ct)
            ?? throw new InvalidOperationException("Хранилище не вернуло ключ объекта для изображения.");

        try
        {
            var thumbData = await thumbnails.GenerateAsync(
                data,
                options.ThumbnailMaxWidth,
                options.ThumbnailMaxHeight,
                ct);

            var thumbFileName = "thumb_" + (image.FileName ?? "image") + ".webp";
            image.ThumbnailPath = await storage.SaveAsync(thumbData, thumbFileName, ct);
        }
        catch
        {
            // Ошибка генерации миниатюры не должна блокировать сохранение оригинала
        }
    }
}
