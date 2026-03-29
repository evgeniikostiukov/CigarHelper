using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CigarHelper.Api.Services;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CigarHelper.Api.Tests;

public class JwtServiceTests
{
    private static IConfiguration CreateJwtConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = new string('k', 32),
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();
    }

    [Fact]
    public void CreatePasswordHash_VerifyPasswordHash_SamePassword_ReturnsTrue()
    {
        JwtService.CreatePasswordHash("secret", out var hash, out var salt);

        Assert.Equal(16, salt.Length);
        Assert.Equal(32, hash.Length);
        Assert.True(JwtService.VerifyPasswordHash("secret", hash, salt, out var rehash));
        Assert.False(rehash);
    }

    [Fact]
    public void CreatePasswordHash_VerifyPasswordHash_WrongPassword_ReturnsFalse()
    {
        JwtService.CreatePasswordHash("secret", out var hash, out var salt);

        Assert.False(JwtService.VerifyPasswordHash("other", hash, salt));
    }

    /// <summary>Старый алгоритм до PBKDF2 — для проверки миграции при логине.</summary>
    private static void CreateLegacyHmac512Hash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    [Fact]
    public void VerifyPasswordHash_LegacyHmac512_Matches_AndRequestsRehash()
    {
        CreateLegacyHmac512Hash("legacy-pw", out var hash, out var salt);

        Assert.True(JwtService.VerifyPasswordHash("legacy-pw", hash, salt, out var rehash));
        Assert.True(rehash);
    }

    [Fact]
    public void VerifyPasswordHash_ModernFormat_DoesNotRequestRehash()
    {
        JwtService.CreatePasswordHash("x", out var hash, out var salt);
        Assert.True(JwtService.VerifyPasswordHash("x", hash, salt, out var rehash));
        Assert.False(rehash);
    }

    [Fact]
    public void GenerateToken_ContainsExpectedClaims_AndValidatesSignature()
    {
        var config = CreateJwtConfiguration();
        var jwtService = new JwtService(config);
        var user = new User
        {
            Id = 42,
            Username = "tester",
            Email = "t@example.com",
            Role = Role.Moderator
        };

        var token = jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "test-issuer",
            ValidateAudience = true,
            ValidAudience = "test-audience",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(new string('k', 32))),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(token, validationParameters, out _);
        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;

        Assert.Equal("42", id);
        Assert.Equal("tester", name);
        Assert.Equal("t@example.com", email);
        Assert.Equal(Role.Moderator.ToString(), role);
        Assert.Equal("42", principal.FindFirst("id")?.Value);
    }

    [Fact]
    public void GenerateToken_WhenKeyMissing_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "x",
                ["Jwt:Audience"] = "y"
            })
            .Build();
        var jwtService = new JwtService(config);
        var user = new User { Id = 1, Username = "u", Email = "e@e.com", Role = Role.User };

        Assert.Throws<InvalidOperationException>(() => jwtService.GenerateToken(user));
    }
}
