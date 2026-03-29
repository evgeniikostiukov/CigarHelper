using CigarHelper.Api.Helpers;
using Xunit;

namespace CigarHelper.Api.Tests;

public class ImageBinaryValidatorTests
{
    private static readonly byte[] JpegStart = [0xFF, 0xD8, 0xFF, 0xE0];
    private static readonly byte[] PngHeader =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00
    ];

    private static readonly byte[] WebpHeader =
    [
        (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0,
        (byte)'W', (byte)'E', (byte)'B', (byte)'P'
    ];

    [Fact]
    public void TryValidate_NullOrEmptyData_Succeeds()
    {
        Assert.True(ImageBinaryValidator.TryValidate(null, "image/png", null, 1000, out var e1));
        Assert.Null(e1);
        Assert.True(ImageBinaryValidator.TryValidate([], "image/jpeg", null, 1000, out var e2));
        Assert.Null(e2);
    }

    [Fact]
    public void TryValidate_JpegWithMatchingMime_Succeeds()
    {
        Assert.True(ImageBinaryValidator.TryValidate(
            JpegStart,
            "image/jpeg",
            JpegStart.LongLength,
            10_000,
            out var err));
        Assert.Null(err);
    }

    [Fact]
    public void TryValidate_JpegWithPngMime_Fails()
    {
        Assert.False(ImageBinaryValidator.TryValidate(
            JpegStart,
            "image/png",
            JpegStart.LongLength,
            10_000,
            out var err));
        Assert.NotNull(err);
    }

    [Fact]
    public void TryValidate_Garbage_Fails()
    {
        Assert.False(ImageBinaryValidator.TryValidate(
            [1, 2, 3, 4],
            null,
            null,
            10_000,
            out var err));
        Assert.NotNull(err);
    }

    [Fact]
    public void TryValidate_ExceedsMaxBytes_Fails()
    {
        Assert.False(ImageBinaryValidator.TryValidate(
            JpegStart,
            "image/jpeg",
            JpegStart.LongLength,
            2,
            out var err));
        Assert.Contains("превышает", err, StringComparison.Ordinal);
    }

    [Fact]
    public void TryValidate_FileSizeMismatch_Fails()
    {
        Assert.False(ImageBinaryValidator.TryValidate(
            JpegStart,
            "image/jpeg",
            99,
            10_000,
            out var err));
        Assert.NotNull(err);
    }

    [Fact]
    public void SuggestContentType_PngAndWebp()
    {
        Assert.Equal("image/png", ImageBinaryValidator.SuggestContentType(PngHeader));
        Assert.Equal("image/webp", ImageBinaryValidator.SuggestContentType(WebpHeader));
    }
}
