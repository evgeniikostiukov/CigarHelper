using CigarHelper.Api.Helpers;
using CigarHelper.Api.Options;
using CigarHelper.Api.Storage;
using CigarHelper.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CigarHelper.Api.Services;

public sealed class UserAvatarService : IUserAvatarService
{
    private const int AvatarMaxEdgePx = 384;

    private readonly AppDbContext _db;
    private readonly IImageStorageProvider _storage;
    private readonly ImageStorageOptions _imageStorage;
    private readonly ImageUploadOptions _upload;
    private readonly ILogger<UserAvatarService> _logger;

    public UserAvatarService(
        AppDbContext db,
        IImageStorageProvider storage,
        IOptions<ImageStorageOptions> imageStorage,
        IOptions<ImageUploadOptions> upload,
        ILogger<UserAvatarService> logger)
    {
        _db = db;
        _storage = storage;
        _imageStorage = imageStorage.Value;
        _upload = upload.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage)> UploadAsync(
        int userId,
        Stream fileStream,
        string? fileName,
        string? declaredContentType,
        long fileLength,
        CancellationToken cancellationToken = default)
    {
        if (fileLength <= 0)
            return (false, "Файл пустой.");

        if (fileLength > _upload.MaxBytes)
            return (false, $"Размер файла превышает лимит ({_upload.MaxBytes} байт).");

        using var buffer = new MemoryStream((int)Math.Min(fileLength, _upload.MaxBytes + 1));
        await fileStream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        if (bytes.Length == 0)
            return (false, "Файл пустой.");
        if (bytes.Length > _upload.MaxBytes)
            return (false, $"Размер файла превышает лимит ({_upload.MaxBytes} байт).");

        var contentType = string.IsNullOrWhiteSpace(declaredContentType)
                          || string.Equals(declaredContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase)
            ? ImageBinaryValidator.SuggestContentType(bytes) ?? "image/jpeg"
            : declaredContentType.Trim();

        if (!ImageBinaryValidator.TryValidate(bytes, contentType, bytes.Length, _upload.MaxBytes, out var validateError))
            return (false, validateError ?? "Недопустимое изображение.");

        var thumb = _imageStorage.ResolvedThumbnailProfile();
        var avatarProfile = new StoredImageEncodingProfile
        {
            Format = "WebP",
            MaxWidth = AvatarMaxEdgePx,
            MaxHeight = AvatarMaxEdgePx,
            WebpQuality = thumb.WebpQuality,
            WebpMethod = thumb.WebpMethod,
            WebpLossless = thumb.WebpLossless,
            WebpNearLossless = thumb.WebpNearLossless,
            WebpNearLosslessQuality = thumb.WebpNearLosslessQuality,
            WebpUseAlphaCompression = thumb.WebpUseAlphaCompression,
            WebpEntropyPasses = thumb.WebpEntropyPasses,
            WebpSpatialNoiseShaping = thumb.WebpSpatialNoiseShaping,
            WebpFilterStrength = thumb.WebpFilterStrength,
            WebpSkipMetadata = thumb.WebpSkipMetadata,
            PreserveGifAsOriginal = false,
            AvifCqLevel = thumb.AvifCqLevel,
        };

        byte[] webp;
        try
        {
            webp = await ImageSharpRasterEncoder.EncodeWebpAsync(bytes, avatarProfile, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось обработать аватар пользователя {UserId}", userId);
            return (false, "Не удалось обработать изображение. Попробуйте другой файл.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
            return (false, "Пользователь не найден.");

        var oldKey = user.AvatarUrl;
        if (!string.IsNullOrWhiteSpace(oldKey) && !UserAvatarPublicUrls.IsExternalHttpUrl(oldKey))
            await _storage.DeleteAsync(oldKey.Trim(), cancellationToken);

        string? newKey;
        try
        {
            newKey = await _storage.SaveAsync(webp, string.IsNullOrWhiteSpace(fileName) ? "avatar.webp" : fileName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка записи аватара в хранилище для пользователя {UserId}", userId);
            return (false, "Не удалось сохранить файл. Повторите позже.");
        }

        if (string.IsNullOrEmpty(newKey))
            return (false, "Хранилище не вернуло ключ объекта.");

        user.AvatarUrl = newKey;
        await _db.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ClearAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
            return (false, "Пользователь не найден.");

        var key = user.AvatarUrl;
        if (!string.IsNullOrWhiteSpace(key) && !UserAvatarPublicUrls.IsExternalHttpUrl(key))
            await _storage.DeleteAsync(key.Trim(), cancellationToken);

        user.AvatarUrl = null;
        await _db.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(byte[]? Data, string ContentType)> GetStoredAvatarAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user?.AvatarUrl is not { Length: > 0 } raw)
            return (null, "image/webp");

        if (UserAvatarPublicUrls.IsExternalHttpUrl(raw))
            return (null, "image/webp");

        var data = await _storage.ReadAsync(raw.Trim(), cancellationToken);
        return (data, "image/webp");
    }
}
