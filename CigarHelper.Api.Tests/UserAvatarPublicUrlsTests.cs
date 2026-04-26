using CigarHelper.Api.Helpers;
using Xunit;

namespace CigarHelper.Api.Tests;

public class UserAvatarPublicUrlsTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    public void ToPublicUrl_Empty_ReturnsNull(string? raw, string? expected)
    {
        Assert.Equal(expected, UserAvatarPublicUrls.ToPublicUrl(7, raw));
    }

    [Fact]
    public void ToPublicUrl_ExternalHttp_Passthrough()
    {
        Assert.Equal("https://cdn.example/a.png", UserAvatarPublicUrls.ToPublicUrl(1, "https://cdn.example/a.png"));
        Assert.Equal("http://x/y", UserAvatarPublicUrls.ToPublicUrl(1, "http://x/y"));
    }

    [Fact]
    public void ToPublicUrl_StorageKey_ReturnsApiPath()
    {
        Assert.Equal("/api/users/42/avatar", UserAvatarPublicUrls.ToPublicUrl(42, "a1b2c3d4e5f6_avatar.webp"));
    }

    [Theory]
    [InlineData("https://x", true)]
    [InlineData("HTTP://X", true)]
    [InlineData("keys_only", false)]
    [InlineData(null, false)]
    public void IsExternalHttpUrl(string? raw, bool expected)
    {
        Assert.Equal(expected, UserAvatarPublicUrls.IsExternalHttpUrl(raw));
    }
}
