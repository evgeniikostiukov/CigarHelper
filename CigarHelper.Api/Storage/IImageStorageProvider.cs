namespace CigarHelper.Api.Storage;

/// <summary>
/// Стратегия хранения бинарных данных изображений вне БД.
/// Реализации: <see cref="MinioImageStorageProvider"/>, <see cref="LocalFileImageStorage"/>.
/// </summary>
public interface IImageStorageProvider
{
    /// <summary>Сохраняет бинарные данные и возвращает ключ объекта для последующего чтения.</summary>
    Task<string?> SaveAsync(byte[] data, string suggestedFileName, CancellationToken ct = default);

    /// <summary>Читает бинарные данные по ключу, полученному от <see cref="SaveAsync"/>.</summary>
    Task<byte[]?> ReadAsync(string storagePath, CancellationToken ct = default);

    /// <summary>Удаляет данные по ключу. Ошибки подавляются логом.</summary>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>Всегда true для текущих реализаций (данные только во внешнем хранилище).</summary>
    bool StoresExternally { get; }
}
