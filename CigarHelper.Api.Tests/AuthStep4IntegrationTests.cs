using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
                Email = "nobody@example.com",
                Password = "abCd12"
            });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        using var limited = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Email = "nobody@example.com",
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
                Email = $"rl{i}@ratelimit.test",
                Password = "abCd12",
                ConfirmPassword = "abCd12"
            });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        using var limited = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = "rlExtra",
            Email = "rlExtra@ratelimit.test",
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401_WithUnifiedMessage()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Email = "missing@example.com",
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
            Email = "has@example.com",
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
            Email = "expchk@example.com",
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });

        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>(AuthResponseJsonOptions);
        Assert.NotNull(body?.Token);
        var validTo = new JwtSecurityTokenHandler().ReadJwtToken(body.Token).ValidTo;
        Assert.Equal(validTo, body!.Expiration);
    }
}
