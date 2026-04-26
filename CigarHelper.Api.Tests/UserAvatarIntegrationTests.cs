using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CigarHelper.Data.Models;
using Xunit;

namespace CigarHelper.Api.Tests;

public class UserAvatarIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Минимальный валидный PNG 1×1.</summary>
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private sealed class MyProfileDtoStub
    {
        public int Id { get; set; }
        public string? AvatarUrl { get; set; }
    }

    [Fact]
    public async Task UploadAvatar_ThenGetBinary_AndProfileShowsPublicPath()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        var username = $"av_{Guid.NewGuid():N}"[..20];
        using var reg = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                Username = username,
                Password = "abCd12",
                ConfirmPassword = "abCd12",
                ConfirmedAge18 = true,
            });
        reg.EnsureSuccessStatusCode();
        var regBody = await reg.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(regBody?.Token);

        using var getProfile = new HttpRequestMessage(HttpMethod.Get, "/api/Profile");
        getProfile.Headers.Authorization = new AuthenticationHeaderValue("Bearer", regBody.Token);
        using var profRes = await client.SendAsync(getProfile);
        profRes.EnsureSuccessStatusCode();
        var me = await profRes.Content.ReadFromJsonAsync<MyProfileDtoStub>(JsonOptions);
        Assert.NotNull(me);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(TinyPng);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "tiny.png");

        using var upload = new HttpRequestMessage(HttpMethod.Post, "/api/Profile/avatar") { Content = content };
        upload.Headers.Authorization = new AuthenticationHeaderValue("Bearer", regBody.Token);
        using var upRes = await client.SendAsync(upload);
        upRes.EnsureSuccessStatusCode();
        var after = await upRes.Content.ReadFromJsonAsync<MyProfileDtoStub>(JsonOptions);
        Assert.NotNull(after?.AvatarUrl);
        Assert.Equal($"/api/users/{me.Id}/avatar", after.AvatarUrl);

        using var img = await client.GetAsync($"/api/users/{me.Id}/avatar");
        img.EnsureSuccessStatusCode();
        var bytes = await img.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 8);
        var media = img.Content.Headers.ContentType?.MediaType;
        Assert.Equal("image/webp", media);

        using var del = new HttpRequestMessage(HttpMethod.Delete, "/api/Profile/avatar");
        del.Headers.Authorization = new AuthenticationHeaderValue("Bearer", regBody.Token);
        using var delRes = await client.SendAsync(del);
        delRes.EnsureSuccessStatusCode();
        using var gone = await client.GetAsync($"/api/users/{me.Id}/avatar");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }
}
