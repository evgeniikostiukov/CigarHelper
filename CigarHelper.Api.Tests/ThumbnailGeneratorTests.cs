using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using CigarHelper.Api.Storage;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>Тесты генерации миниатюр через ImageSharpThumbnailGenerator.</summary>
public class ThumbnailGeneratorTests
{
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

        var thumb = await _generator.GenerateAsync(source, 320, 320);

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

        var thumb = await _generator.GenerateAsync(source, 320, 320);

        using var result = Image.Load(thumb);
        // Изображение меньше лимита — размер не меняется
        Assert.Equal(100, result.Width);
        Assert.Equal(100, result.Height);
    }

    [Fact]
    public async Task GenerateAsync_WideImage_MaintainsAspectRatio()
    {
        var source = CreateTestJpeg(800, 200);

        var thumb = await _generator.GenerateAsync(source, 320, 320);

        using var result = Image.Load(thumb);
        // Ограничение по ширине: 800 → 320, высота пропорционально 200 * (320/800) = 80
        Assert.Equal(320, result.Width);
        Assert.Equal(80, result.Height);
    }

    [Fact]
    public async Task GenerateAsync_Output_IsWebP()
    {
        var source = CreateTestJpeg(400, 300);

        var thumb = await _generator.GenerateAsync(source, 320, 320);

        // WebP сигнатура: RIFF....WEBP
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
}
