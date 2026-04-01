namespace CigarHelper.Api.Storage;

/// <summary>Генерирует миниатюру из бинарных данных изображения.</summary>
public interface IThumbnailGenerator
{
    /// <summary>
    /// Создаёт миниатюру с вписыванием в ограничивающий прямоугольник <paramref name="maxWidth"/> x <paramref name="maxHeight"/>.
    /// Соотношение сторон сохраняется. Возвращает WebP-байты миниатюры.
    /// </summary>
    Task<byte[]> GenerateAsync(
        byte[] sourceData,
        int maxWidth,
        int maxHeight,
        CancellationToken ct = default);
}
