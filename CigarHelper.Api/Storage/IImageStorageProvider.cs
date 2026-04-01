namespace CigarHelper.Api.Storage;

/// <summary>
/// Стратегия хранения бинарных данных изображений.
/// Реализации: <see cref="DatabaseImageStorage"/> (данные в Postgres bytea),
/// <see cref="LocalFileImageStorage"/> (данные на диске).
/// </summary>
public interface IImageStorageProvider
{
    /// <summary>
    /// Сохраняет бинарные данные и возвращает ключ (путь) для последующего чтения.
    /// Для <see cref="DatabaseImageStorage"/> ключ — null (данные вшиты в сущность).
    /// </summary>
    Task<string?> SaveAsync(byte[] data, string suggestedFileName, CancellationToken ct = default);

    /// <summary>Читает бинарные данные по ключу, полученному от <see cref="SaveAsync"/>.</summary>
    Task<byte[]?> ReadAsync(string storagePath, CancellationToken ct = default);

    /// <summary>Удаляет данные по ключу. Ошибки файловой системы подавляются логом.</summary>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// True если provider хранит данные вне EF-сущности (на диске / в объектном хранилище).
    /// При false данные пишутся напрямую в поля <c>ImageData</c>/<c>ThumbnailData</c> сущности.
    /// </summary>
    bool StoresExternally { get; }
}
