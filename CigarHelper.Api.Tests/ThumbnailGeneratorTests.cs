using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using CigarHelper.Api.Options;
using CigarHelper.Api.Storage;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>Тесты генерации миниатюр через ImageSharpThumbnailGenerator.</summary>
public class ThumbnailGeneratorTests
{
    private static ImageStorageOptions ThumbOptions(int w = 320, int h = 320) =>
        new()
        {
            ThumbnailMaxWidth = w,
            ThumbnailMaxHeight = h,
        };

    private readonly ImageSharpThumbnailGenerator _generator = new();

    private static byte[] CreateTestJpeg(int width, int height)
    {
        using var image = new Image<Rgb24>(width, height);
        using var ms = new MemoryStream();
        image.Save(ms, new JpegEncoder());
        return ms.ToArray();
    }

    [Fact]
    public async Task GenerateAsync_LargeImage_ReturnsSmallerWebP()
    {
        var source = CreateTestJpeg(1200, 900);
        var opts = ThumbOptions();

        var thumb = await _generator.GenerateAsync(source, opts);

        Assert.NotNull(thumb);
        Assert.True(thumb.Length > 0);

        using var result = Image.Load(thumb);
        Assert.True(result.Width <= 320);
        Assert.True(result.Height <= 320);
    }

    [Fact]
    public async Task GenerateAsync_SmallImage_DoesNotUpscale()
    {
        var source = CreateTestJpeg(100, 100);
        var opts = ThumbOptions();

        var thumb = await _generator.GenerateAsync(source, opts);

        using var result = Image.Load(thumb);
        Assert.Equal(100, result.Width);
        Assert.Equal(100, result.Height);
    }

    [Fact]
    public async Task GenerateAsync_WideImage_MaintainsAspectRatio()
    {
        var source = CreateTestJpeg(800, 200);
        var opts = ThumbOptions();

        var thumb = await _generator.GenerateAsync(source, opts);

        using var result = Image.Load(thumb);
        Assert.Equal(320, result.Width);
        Assert.Equal(80, result.Height);
    }

    [Fact]
    public async Task GenerateAsync_Output_IsWebP()
    {
        var source = CreateTestJpeg(400, 300);
        var opts = ThumbOptions();

        var thumb = await _generator.GenerateAsync(source, opts);

        Assert.True(thumb.Length >= 12);
        Assert.Equal((byte)'R', thumb[0]);
        Assert.Equal((byte)'I', thumb[1]);
        Assert.Equal((byte)'F', thumb[2]);
        Assert.Equal((byte)'F', thumb[3]);
        Assert.Equal((byte)'W', thumb[8]);
        Assert.Equal((byte)'E', thumb[9]);
        Assert.Equal((byte)'B', thumb[10]);
        Assert.Equal((byte)'P', thumb[11]);
    }

    [Fact]
    public async Task PrepareOriginalAsync_WebP_DownscalesToMaxBox()
    {
        var source = CreateTestJpeg(3000, 2000);
        var options = new ImageStorageOptions
        {
            Compression = new ImageCompressionOptions
            {
                Original = new StoredImageEncodingProfile
                {
                    Format = "WebP",
                    MaxWidth = 2048,
                    MaxHeight = 2048,
                    WebpQuality = 82,
                    WebpMethod = 4,
                },
            },
        };

        var (blob, name, mime) = await CigarImageOriginalPipeline.PrepareOriginalAsync(
            source,
            "huge.jpg",
            "image/jpeg",
            options);

        Assert.Equal("image/webp", mime);
        Assert.EndsWith(".webp", name, StringComparison.OrdinalIgnoreCase);
        using var img = Image.Load(blob);
        Assert.True(img.Width <= 2048);
        Assert.True(img.Height <= 2048);
    }

    [Fact]
    public async Task PrepareOriginalAsync_Avif_ProducesAvifAndFtyp()
    {
        var source = CreateTestJpeg(400, 300);
        var options = new ImageStorageOptions
        {
            Compression = new ImageCompressionOptions
            {
                Original = new StoredImageEncodingProfile
                {
                    Format = "Avif",
                    MaxWidth = 2048,
                    MaxHeight = 2048,
                    AvifCqLevel = 28,
                },
            },
        };

        var (blob, name, mime) = await CigarImageOriginalPipeline.PrepareOriginalAsync(
            source,
            "photo.jpg",
            "image/jpeg",
            options);

        Assert.Equal("image/avif", mime);
        Assert.EndsWith(".avif", name, StringComparison.OrdinalIgnoreCase);
        Assert.True(blob.Length >= 12);
        Assert.Equal((byte)'f', blob[4]);
        Assert.Equal((byte)'t', blob[5]);
        Assert.Equal((byte)'y', blob[6]);
        Assert.Equal((byte)'p', blob[7]);
        Assert.Equal((byte)'a', blob[8]);
        Assert.Equal((byte)'v', blob[9]);
        Assert.Equal((byte)'i', blob[10]);
        Assert.Equal((byte)'f', blob[11]);
    }
}
