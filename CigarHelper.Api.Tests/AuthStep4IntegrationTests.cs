using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>Шаг 4: единый ответ логина и rate limiting (интеграционно).</summary>
public class AuthStep4IntegrationTests
{
    /// <summary>Совпадает с AddJsonOptions в Program (строковые enum, camelCase свойств).</summary>
    private static readonly JsonSerializerOptions AuthResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Login_Returns429_On21stRequest_InSameWindow()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        for (var i = 0; i < 20; i++)
        {
            using var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
            {
                Username = "nobody",
                Password = "abCd12"
            });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        using var limited = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Username = "nobody",
            Password = "abCd12"
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    [Fact]
    public async Task Register_Returns429_On11thRequest_InSameWindow()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        for (var i = 0; i < 10; i++)
        {
            using var response = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
            {
                Username = $"rl{i}",
                Password = "abCd12",
                ConfirmPassword = "abCd12",
            ConfirmedAge18 = true
            });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        using var limited = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = "rlExtra",
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = true
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    [Fact]
    public async Task Register_WithoutAgeConfirmation_Returns400()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = "noage18",
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownUsername_Returns401_WithUnifiedMessage()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Username = "missing_user",
            Password = "abCd12"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(AuthResponseJsonOptions);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal(AuthService.LoginFailedMessage, body.Message);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401_WithUnifiedMessage()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
            db.Users.Add(new User
            {
                Username = "hasuser",
                Email = "has@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Username = "hasuser",
            Password = "wrongPw9"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(AuthResponseJsonOptions);
        Assert.NotNull(body);
        Assert.Equal(AuthService.LoginFailedMessage, body.Message);
    }

    [Fact]
    public async Task Register_Response_ExpirationMatchesJwt_ValidTo()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = "expchk",
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = true
        });

        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>(AuthResponseJsonOptions);
        Assert.NotNull(body?.Token);
        var validTo = new JwtSecurityTokenHandler().ReadJwtToken(body.Token).ValidTo;
        Assert.Equal(validTo, body!.Expiration);
    }

    [Fact]
    public async Task Refresh_WithoutToken_Returns401()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/Auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewJwt_AndExpirationMatches()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var reg = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = "refreshme",
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = true
        });
        reg.EnsureSuccessStatusCode();
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponse>(AuthResponseJsonOptions);
        Assert.NotNull(auth?.Token);

        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/refresh");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        using var refresh = await client.SendAsync(req);

        refresh.EnsureSuccessStatusCode();
        var body = await refresh.Content.ReadFromJsonAsync<AuthResponse>(AuthResponseJsonOptions);
        Assert.NotNull(body?.Token);
        Assert.True(body.Success);
        var validTo = new JwtSecurityTokenHandler().ReadJwtToken(body.Token).ValidTo;
        Assert.Equal(validTo, body.Expiration);
    }
}
