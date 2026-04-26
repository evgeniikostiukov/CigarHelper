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
        var (blob, suggestedFileName, contentType) = await CigarImageOriginalPipeline.PrepareOriginalAsync(
            data,
            image.FileName,
            image.ContentType,
            options,
            ct);

        image.FileName = suggestedFileName;
        image.ContentType = contentType;
        image.FileSize = blob.Length;

        image.StoragePath = await storage.SaveAsync(blob, suggestedFileName, ct)
            ?? throw new InvalidOperationException("Хранилище не вернуло ключ объекта для изображения.");

        try
        {
            var thumbData = await thumbnails.GenerateAsync(blob, options, ct);
            var thumbStem = Path.GetFileNameWithoutExtension(suggestedFileName);
            if (thumbStem.Length == 0)
                thumbStem = "image";
            var thumbFileName = "thumb_" + thumbStem + ".webp";
            image.ThumbnailPath = await storage.SaveAsync(thumbData, thumbFileName, ct);
        }
        catch
        {
            // Ошибка генерации миниатюры не должна блокировать сохранение оригинала
        }
    }
}
