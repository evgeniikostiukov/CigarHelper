using CigarHelper.Api.Options;
using NeoSolve.ImageSharp.AVIF;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Декодирование, AutoOrient, вписывание в прямоугольник и сохранение в WebP или AVIF по профилю <see cref="StoredImageEncodingProfile"/>.
/// </summary>
public static class ImageSharpRasterEncoder
{
    static ImageSharpRasterEncoder() => AvifImageSharpBootstrap.EnsureConfigured();

    public static async Task<byte[]> EncodeWebpAsync(
        byte[] sourceData,
        StoredImageEncodingProfile profile,
        CancellationToken ct = default)
    {
        using var image = Image.Load(sourceData);
        ApplyAutoOrientAndResize(image, profile);

        var encoder = CreateWebpEncoder(profile);
        using var ms = new MemoryStream();
        await image.SaveAsync(ms, encoder, ct);
        return ms.ToArray();
    }

    /// <summary>Перекодирование в AVIF (CQ 0–63: меньше — лучше качество, больше размер).</summary>
    public static async Task<byte[]> EncodeAvifAsync(
        byte[] sourceData,
        StoredImageEncodingProfile profile,
        CancellationToken ct = default)
    {
        using var image = Image.Load(sourceData);
        ApplyAutoOrientAndResize(image, profile);

        var cq = Math.Clamp(profile.AvifCqLevel, 0, 63);
        var encoder = new AVIFEncoder { CQLevel = cq };
        using var ms = new MemoryStream();
        await image.SaveAsync(ms, encoder, ct);
        return ms.ToArray();
    }

    private static void ApplyAutoOrientAndResize(Image image, StoredImageEncodingProfile profile)
    {
        image.Mutate(x => x.AutoOrient());

        var maxW = profile.MaxWidth;
        var maxH = profile.MaxHeight;
        var (origW, origH) = (image.Width, image.Height);
        if (maxW > 0 && maxH > 0 && (origW > maxW || origH > maxH))
        {
            var ratioW = (double)maxW / origW;
            var ratioH = (double)maxH / origH;
            var ratio = Math.Min(ratioW, ratioH);
            var newW = Math.Max(1, (int)(origW * ratio));
            var newH = Math.Max(1, (int)(origH * ratio));
            image.Mutate(x => x.Resize(newW, newH));
        }
        else if (maxW > 0 && origW > maxW)
        {
            var newW = maxW;
            var newH = Math.Max(1, (int)(origH * ((double)maxW / origW)));
            image.Mutate(x => x.Resize(newW, newH));
        }
        else if (maxH > 0 && origH > maxH)
        {
            var newH = maxH;
            var newW = Math.Max(1, (int)(origW * ((double)maxH / origH)));
            image.Mutate(x => x.Resize(newW, newH));
        }
    }

    private static WebpEncoder CreateWebpEncoder(StoredImageEncodingProfile profile)
    {
        var method = (WebpEncodingMethod)Math.Clamp(profile.WebpMethod, 0, 6);
        if (profile.WebpLossless)
        {
            return new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossless,
                Method = method,
                SkipMetadata = profile.WebpSkipMetadata,
            };
        }

        var q = Math.Clamp(profile.WebpQuality, 1, 100);
        var nlq = Math.Clamp(profile.WebpNearLosslessQuality, 0, 100);

        if (profile.WebpNearLossless)
        {
            return new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossy,
                Quality = q,
                Method = method,
                NearLossless = true,
                NearLosslessQuality = nlq,
                UseAlphaCompression = profile.WebpUseAlphaCompression,
                EntropyPasses = (byte)Math.Clamp(profile.WebpEntropyPasses, 0, 10),
                SpatialNoiseShaping = (byte)Math.Clamp(profile.WebpSpatialNoiseShaping, 0, 100),
                FilterStrength = (byte)Math.Clamp(profile.WebpFilterStrength, 0, 100),
                SkipMetadata = profile.WebpSkipMetadata,
            };
        }

        return new WebpEncoder
        {
            FileFormat = WebpFileFormatType.Lossy,
            Quality = q,
            Method = method,
            UseAlphaCompression = profile.WebpUseAlphaCompression,
            EntropyPasses = (byte)Math.Clamp(profile.WebpEntropyPasses, 0, 10),
            SpatialNoiseShaping = (byte)Math.Clamp(profile.WebpSpatialNoiseShaping, 0, 100),
            FilterStrength = (byte)Math.Clamp(profile.WebpFilterStrength, 0, 100),
            SkipMetadata = profile.WebpSkipMetadata,
        };
    }
}
