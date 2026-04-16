using CigarHelper.Api.Options;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Реализация <see cref="IThumbnailGenerator"/> на базе SixLabors.ImageSharp.
/// Перед ресайзом применяет EXIF Orientation (часто у фото с телефонов), затем WebP по профилю из конфигурации.
/// </summary>
public sealed class ImageSharpThumbnailGenerator : IThumbnailGenerator
{
    public Task<byte[]> GenerateAsync(
        byte[] sourceData,
        ImageStorageOptions imageStorageOptions,
        CancellationToken ct = default)
    {
        var profile = imageStorageOptions.ResolvedThumbnailProfile();
        if (profile.Format.Equals("Avif", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "ImageStorage:Compression:Thumbnail:Format=Avif в API не поддерживается. Используйте WebP.");
        }

        return ImageSharpRasterEncoder.EncodeWebpAsync(sourceData, profile, ct);
    }
}
