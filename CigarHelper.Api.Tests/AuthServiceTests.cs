using System.Security.Cryptography;
using System.Text;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CigarHelper.Api.Tests;

public class AuthServiceTests
{
    private const string ValidPassword = "abCd12";
    private static RegisterRequest NewRegisterRequest(string username) => new()
    {
        Username = username,
        Password = ValidPassword,
        ConfirmPassword = ValidPassword,
        ConfirmedAge18 = true
    };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Auth_{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task RegisterAsync_Success_PersistsUser_AndReturnsMockToken()
    {
        await using var context = CreateContext();
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var exp = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("mock-token", exp));
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.RegisterAsync(NewRegisterRequest("newuser"));

        Assert.True(res.Success);
        Assert.Equal("mock-token", res.Token);
        Assert.Equal(exp, res.Expiration);
        Assert.Equal("newuser", res.Username);
        Assert.Equal(Role.User, res.Role);
        Assert.Single(context.Users);
        var stored = await context.Users.SingleAsync();
        Assert.Null(stored.Email);
        jwt.Verify(j => j.GenerateToken(It.Is<User>(u => u.Username == "newuser" && u.Email == null)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFailure()
    {
        await using var context = CreateContext();
        JwtService.CreatePasswordHash(ValidPassword, out var hash, out var salt);
        context.Users.Add(new User
        {
            Username = "taken",
            Email = "a@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.RegisterAsync(NewRegisterRequest("taken"));

        Assert.False(res.Success);
        Assert.Equal("Username already taken", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_AgeNotConfirmed_ReturnsFailure()
    {
        await using var context = CreateContext();
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new AuthService(context, jwt.Object);

        var req = NewRegisterRequest("agegate");
        req.ConfirmedAge18 = false;

        var res = await sut.RegisterAsync(req);

        Assert.False(res.Success);
        Assert.Equal(AuthValidationMessages.ConfirmedAge18, res.Message);
        Assert.Empty(context.Users);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsMockToken_AndSetsLastLogin()
    {
        await using var context = CreateContext();
        JwtService.CreatePasswordHash(ValidPassword, out var hash, out var salt);
        var before = DateTime.UtcNow.AddMinutes(-5);
        context.Users.Add(new User
        {
            Username = "loguser",
            Email = "log@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = before,
            LastLogin = null
        });
        await context.SaveChangesAsync();

        var jwt = new Mock<IJwtService>();
        var exp = DateTime.UtcNow.AddDays(1);
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("login-token", exp));
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.LoginAsync(new LoginRequest { Username = "loguser", Password = ValidPassword });

        Assert.True(res.Success);
        Assert.Equal("login-token", res.Token);
        Assert.Equal(exp, res.Expiration);
        Assert.Equal("loguser", res.Username);
        var reloaded = await context.Users.SingleAsync();
        Assert.NotNull(reloaded.LastLogin);
        Assert.True(reloaded.LastLogin >= before);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFailure()
    {
        await using var context = CreateContext();
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.LoginAsync(new LoginRequest { Username = "nosuchuser", Password = ValidPassword });

        Assert.False(res.Success);
        Assert.Equal(AuthService.LoginFailedMessage, res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_LegacyPasswordHash_UpgradesToPbkdf2_OnSuccess()
    {
        await using var context = CreateContext();
        using (var hmac = new HMACSHA512())
        {
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(ValidPassword));
            context.Users.Add(new User
            {
                Username = "legacy",
                Email = "leg@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        var jwt = new Mock<IJwtService>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("t", DateTime.UtcNow.AddHours(1)));
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.LoginAsync(new LoginRequest { Username = "legacy", Password = ValidPassword });

        Assert.True(res.Success);
        var user = await context.Users.SingleAsync();
        Assert.Equal(16, user.PasswordSalt.Length);
        Assert.Equal(32, user.PasswordHash.Length);
        Assert.True(JwtService.VerifyPasswordHash(ValidPassword, user.PasswordHash, user.PasswordSalt, out var rehash));
        Assert.False(rehash);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        await using var context = CreateContext();
        JwtService.CreatePasswordHash(ValidPassword, out var hash, out var salt);
        context.Users.Add(new User
        {
            Username = "u",
            Email = "e@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.LoginAsync(new LoginRequest { Username = "u", Password = "wrongPw1" });

        Assert.False(res.Success);
        Assert.Equal(AuthService.LoginFailedMessage, res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserNotFound_ReturnsFailure()
    {
        await using var context = CreateContext();
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.RefreshTokenAsync(999);

        Assert.False(res.Success);
        Assert.Equal("User not found", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidUser_ReturnsMockToken_AndSetsLastLogin()
    {
        await using var context = CreateContext();
        JwtService.CreatePasswordHash(ValidPassword, out var hash, out var salt);
        var before = DateTime.UtcNow.AddMinutes(-5);
        context.Users.Add(new User
        {
            Username = "refuser",
            Email = "ref@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = before,
            LastLogin = null
        });
        await context.SaveChangesAsync();
        var userId = (await context.Users.SingleAsync()).Id;

        var jwt = new Mock<IJwtService>();
        var exp = DateTime.UtcNow.AddDays(1);
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("refresh-token", exp));
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.RefreshTokenAsync(userId);

        Assert.True(res.Success);
        Assert.Equal("refresh-token", res.Token);
        Assert.Equal(exp, res.Expiration);
        Assert.Equal("refuser", res.Username);
        var reloaded = await context.Users.SingleAsync();
        Assert.NotNull(reloaded.LastLogin);
        Assert.True(reloaded.LastLogin >= before);
        jwt.Verify(j => j.GenerateToken(It.Is<User>(u => u.Id == userId && u.Username == "refuser")), Times.Once);
    }
}
