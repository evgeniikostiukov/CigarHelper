namespace CigarHelper.Api.Services;

public interface IUserAvatarService
{
    /// <summary>Валидация, ресайз WebP, запись в хранилище, обновление <c>User.AvatarUrl</c>.</summary>
    Task<(bool Success, string? ErrorMessage)> UploadAsync(int userId, Stream fileStream, string? fileName, string? declaredContentType, long fileLength, CancellationToken cancellationToken = default);

    /// <summary>Удаляет файл из хранилища (если ключ) и сбрасывает поле.</summary>
    Task<(bool Success, string? ErrorMessage)> ClearAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Байты аватара из хранилища; null если нет или внешний URL.</summary>
    Task<(byte[]? Data, string ContentType)> GetStoredAvatarAsync(int userId, CancellationToken cancellationToken = default);
}
