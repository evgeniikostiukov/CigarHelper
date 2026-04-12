using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Реализация <see cref="IThumbnailGenerator"/> на базе SixLabors.ImageSharp.
/// Перед ресайзом применяет EXIF Orientation (часто у фото с телефонов), затем WebP.
/// </summary>
public sealed class ImageSharpThumbnailGenerator : IThumbnailGenerator
{
    public async Task<byte[]> GenerateAsync(
        byte[] sourceData,
        int maxWidth,
        int maxHeight,
        CancellationToken ct = default)
    {
        using var image = Image.Load(sourceData);
        image.Mutate(x => x.AutoOrient());

        var (origW, origH) = (image.Width, image.Height);
        if (origW > maxWidth || origH > maxHeight)
        {
            var ratioW = (double)maxWidth / origW;
            var ratioH = (double)maxHeight / origH;
            var ratio = Math.Min(ratioW, ratioH);
            var newW = Math.Max(1, (int)(origW * ratio));
            var newH = Math.Max(1, (int)(origH * ratio));
            image.Mutate(x => x.Resize(newW, newH));
        }

        using var ms = new MemoryStream();
        var encoder = new WebpEncoder { Quality = 80 };
        await image.SaveAsync(ms, encoder, ct);
        return ms.ToArray();
    }
}
