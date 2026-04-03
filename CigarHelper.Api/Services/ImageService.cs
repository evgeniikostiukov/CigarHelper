using CigarHelper.Api.Options;
using CigarHelper.Api.Storage;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using Microsoft.Extensions.Options;

namespace CigarHelper.Api.Services;

/// <summary>
/// Оркестрирует сохранение изображений: валидация → MinIO/файлы → миниатюра WebP.
/// </summary>
public sealed class ImageService : IImageService
{
    private readonly AppDbContext _db;
    private readonly IImageStorageProvider _storage;
    private readonly IThumbnailGenerator _thumbnails;
    private readonly ImageStorageOptions _options;

    public ImageService(
        AppDbContext db,
        IImageStorageProvider storage,
        IThumbnailGenerator thumbnails,
        IOptions<ImageStorageOptions> options)
    {
        _db = db;
        _storage = storage;
        _thumbnails = thumbnails;
        _options = options.Value;
    }

    public async Task<CigarImage> SaveImageAsync(
        byte[]? imageData,
        string? contentType,
        string? fileName,
        string? description,
        bool isMain,
        int? cigarBaseId,
        int? userCigarId,
        CancellationToken ct = default)
    {
        var image = new CigarImage
        {
            FileName = fileName,
            ContentType = contentType,
            FileSize = imageData?.Length,
            Description = description,
            IsMain = isMain,
            CigarBaseId = cigarBaseId,
            UserCigarId = userCigarId,
            CreatedAt = DateTime.UtcNow
        };

        if (imageData is { Length: > 0 })
            await WriteDataAsync(image, imageData, ct);

        _db.CigarImages.Add(image);
        await _db.SaveChangesAsync(ct);

        return image;
    }

    public async Task UpdateImageDataAsync(
        CigarImage image,
        byte[] newData,
        string? contentType,
        CancellationToken ct = default)
    {
        if (image.StoragePath is not null)
            await _storage.DeleteAsync(image.StoragePath, ct);
        if (image.ThumbnailPath is not null)
            await _storage.DeleteAsync(image.ThumbnailPath, ct);

        if (contentType is not null)
            image.ContentType = contentType;

        await WriteDataAsync(image, newData, ct);
        image.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteImageAsync(CigarImage image, CancellationToken ct = default)
    {
        if (image.StoragePath is not null)
            await _storage.DeleteAsync(image.StoragePath, ct);
        if (image.ThumbnailPath is not null)
            await _storage.DeleteAsync(image.ThumbnailPath, ct);

        _db.CigarImages.Remove(image);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(byte[]? Data, string ContentType)> GetImageDataAsync(
        CigarImage image,
        CancellationToken ct = default)
    {
        var contentType = image.ContentType ?? "image/jpeg";

        if (image.StoragePath is null)
            return (null, contentType);

        var data = await _storage.ReadAsync(image.StoragePath, ct);
        return (data, contentType);
    }

    public async Task<byte[]?> GetThumbnailDataAsync(CigarImage image, CancellationToken ct = default)
    {
        if (image.ThumbnailPath is null)
            return null;

        return await _storage.ReadAsync(image.ThumbnailPath, ct);
    }

    private async Task WriteDataAsync(CigarImage image, byte[] data, CancellationToken ct)
    {
        image.FileSize = data.Length;

        image.StoragePath = await _storage.SaveAsync(data, image.FileName ?? "image", ct)
            ?? throw new InvalidOperationException("Хранилище не вернуло ключ объекта для изображения.");

        try
        {
            var thumbData = await _thumbnails.GenerateAsync(
                data,
                _options.ThumbnailMaxWidth,
                _options.ThumbnailMaxHeight,
                ct);

            var thumbFileName = "thumb_" + (image.FileName ?? "image") + ".webp";
            image.ThumbnailPath = await _storage.SaveAsync(thumbData, thumbFileName, ct);
        }
        catch (Exception)
        {
            // Ошибка генерации миниатюры не должна блокировать сохранение оригинала
        }
    }
}
