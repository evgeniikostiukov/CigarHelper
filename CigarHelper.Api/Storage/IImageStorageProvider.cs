namespace CigarHelper.Api.Storage;

/// <summary>Метаданные объекта в хранилище (без загрузки тела).</summary>
public sealed record ImageStorageObjectInfo(long Size, string? ContentType);

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

    /// <summary>
    /// Проверяет наличие объекта по ключу (включая вложенный путь, например <c>import/…</c> для идемпотентного импорта).
    /// </summary>
    Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Сохраняет данные по фиксированному ключу без генерации GUID (импорт CSV и другие идемпотентные сценарии).
    /// </summary>
    Task PutAtKeyAsync(byte[] data, string storagePath, string contentType, CancellationToken ct = default);

    /// <summary>Размер и Content-Type по ключу; если объекта нет — null (без чтения байтов в память).</summary>
    Task<ImageStorageObjectInfo?> TryDescribeAsync(string storagePath, CancellationToken ct = default);

    /// <summary>Всегда true для текущих реализаций (данные только во внешнем хранилище).</summary>
    bool StoresExternally { get; }
}
