namespace CigarHelper.Api.Storage;

/// <summary>
/// Хранение изображений в самой EF-сущности (поле bytea в Postgres).
/// Используется по умолчанию. Удобно для малых объёмов, не требует доп. инфраструктуры.
/// Минус: раздувает таблицу, замедляет запросы без SELECT по binary.
/// </summary>
public sealed class DatabaseImageStorage : IImageStorageProvider
{
    public bool StoresExternally => false;

    public Task<string?> SaveAsync(byte[] data, string suggestedFileName, CancellationToken ct = default)
        => Task.FromResult<string?>(null);

    public Task<byte[]?> ReadAsync(string storagePath, CancellationToken ct = default)
        => Task.FromResult<byte[]?>(null);

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
        => Task.CompletedTask;
}
