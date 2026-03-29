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
    private static RegisterRequest NewRegisterRequest(string username, string email) => new()
    {
        Username = username,
        Email = email,
        Password = ValidPassword,
        ConfirmPassword = ValidPassword
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
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("mock-token");
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.RegisterAsync(NewRegisterRequest("newuser", "new@example.com"));

        Assert.True(res.Success);
        Assert.Equal("mock-token", res.Token);
        Assert.Equal("newuser", res.Username);
        Assert.Equal(Role.User, res.Role);
        Assert.Single(context.Users);
        jwt.Verify(j => j.GenerateToken(It.Is<User>(u => u.Username == "newuser" && u.Email == "new@example.com")), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        await using var context = CreateContext();
        JwtService.CreatePasswordHash(ValidPassword, out var hash, out var salt);
        context.Users.Add(new User
        {
            Username = "other",
            Email = "dup@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.RegisterAsync(NewRegisterRequest("newuser", "dup@example.com"));

        Assert.False(res.Success);
        Assert.Equal("Email already registered", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
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

        var res = await sut.RegisterAsync(NewRegisterRequest("taken", "b@example.com"));

        Assert.False(res.Success);
        Assert.Equal("Username already taken", res.Message);
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
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("login-token");
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.LoginAsync(new LoginRequest { Email = "log@example.com", Password = ValidPassword });

        Assert.True(res.Success);
        Assert.Equal("login-token", res.Token);
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

        var res = await sut.LoginAsync(new LoginRequest { Email = "none@example.com", Password = ValidPassword });

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
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("t");
        var sut = new AuthService(context, jwt.Object);

        var res = await sut.LoginAsync(new LoginRequest { Email = "leg@example.com", Password = ValidPassword });

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

        var res = await sut.LoginAsync(new LoginRequest { Email = "e@example.com", Password = "wrongPw1" });

        Assert.False(res.Success);
        Assert.Equal(AuthService.LoginFailedMessage, res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}
