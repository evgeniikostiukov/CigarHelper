using CigarHelper.Api.Models;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// Покрытие профиля: чтение «моего» профиля, обновление с уникальностью и выпуском JWT,
/// смена пароля с кэш-лимитом, публичный профиль и доступ к хьюмидору через IHumidorService.
/// </summary>
public class ProfileServiceTests
{
    private const string ValidPassword = "abCd12";
    private const string NewValidPassword = "xyZa99";

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Profile_{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<User> SeedUserAsync(
        AppDbContext db,
        string username,
        string? email,
        bool isProfilePublic = true)
    {
        JwtService.CreatePasswordHash(ValidPassword, out var hash, out var salt);
        var u = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            IsProfilePublic = isProfilePublic,
            Role = Role.User
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return u;
    }

    private static ProfileService CreateSut(
        AppDbContext db,
        Mock<IJwtService>? jwt = null,
        Mock<IHumidorService>? humidor = null,
        IMemoryCache? cache = null)
    {
        jwt ??= new Mock<IJwtService>();
        humidor ??= new Mock<IHumidorService>();
        cache ??= new MemoryCache(new MemoryCacheOptions());
        return new ProfileService(db, jwt.Object, humidor.Object, cache);
    }

    [Fact]
    public async Task GetMyProfileAsync_UserExists_ReturnsMappedDto()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "me", "me@example.com");
        var sut = CreateSut(db);

        var dto = await sut.GetMyProfileAsync(user.Id);

        Assert.NotNull(dto);
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal("me", dto.Username);
        Assert.Equal("me@example.com", dto.Email);
        Assert.Equal(Role.User, dto.Role);
    }

    [Fact]
    public async Task GetMyProfileAsync_UserWithoutEmail_ReturnsEmptyEmailInDto()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "noemail", email: null);
        var sut = CreateSut(db);

        var dto = await sut.GetMyProfileAsync(user.Id);

        Assert.NotNull(dto);
        Assert.Equal("", dto!.Email);
    }

    [Fact]
    public async Task GetMyProfileAsync_UnknownId_ReturnsNull()
    {
        await using var db = CreateContext();
        var sut = CreateSut(db);

        var dto = await sut.GetMyProfileAsync(999);

        Assert.Null(dto);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsFailure()
    {
        await using var db = CreateContext();
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = CreateSut(db, jwt);

        var res = await sut.UpdateProfileAsync(1, new UpdateProfileRequest
        {
            Username = "newname",
            Email = "a@b.com",
            IsProfilePublic = true
        });

        Assert.False(res.Success);
        Assert.Equal("Пользователь не найден", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_UsernameAlreadyTaken_ReturnsFailure()
    {
        await using var db = CreateContext();
        await SeedUserAsync(db, "taken", "a@example.com");
        var current = await SeedUserAsync(db, "self", "self@example.com");
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = CreateSut(db, jwt);

        var res = await sut.UpdateProfileAsync(current.Id, new UpdateProfileRequest
        {
            Username = "taken",
            Email = "self@example.com",
            IsProfilePublic = true
        });

        Assert.False(res.Success);
        Assert.Equal("Имя пользователя уже занято", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_EmailAlreadyRegistered_ReturnsFailure()
    {
        await using var db = CreateContext();
        await SeedUserAsync(db, "other", "taken@example.com");
        var current = await SeedUserAsync(db, "self", "self@example.com");
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = CreateSut(db, jwt);

        var res = await sut.UpdateProfileAsync(current.Id, new UpdateProfileRequest
        {
            Username = "self",
            Email = "taken@example.com",
            IsProfilePublic = true
        });

        Assert.False(res.Success);
        Assert.Equal("Этот email уже зарегистрирован", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_OnlyVisibilityChanged_DoesNotIssueNewToken()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "same", "same@example.com", isProfilePublic: false);
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = CreateSut(db, jwt);

        var res = await sut.UpdateProfileAsync(user.Id, new UpdateProfileRequest
        {
            Username = "same",
            Email = "same@example.com",
            IsProfilePublic = true
        });

        Assert.True(res.Success);
        Assert.Null(res.NewToken);
        Assert.NotNull(res.Profile);
        Assert.True(res.Profile!.IsProfilePublic);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_EmailChanged_CallsJwt_AndReturnsNewToken()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "u1", "old@example.com");
        var jwt = new Mock<IJwtService>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("fresh-jwt", DateTime.UtcNow));
        var sut = CreateSut(db, jwt);

        var res = await sut.UpdateProfileAsync(user.Id, new UpdateProfileRequest
        {
            Username = "u1",
            Email = "new@example.com",
            IsProfilePublic = true
        });

        Assert.True(res.Success);
        Assert.Equal("fresh-jwt", res.NewToken);
        jwt.Verify(j => j.GenerateToken(It.Is<User>(u => u.Email == "new@example.com")), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_UsernameChanged_CallsJwt_AndReturnsNewToken()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "oldname", "mail@example.com");
        var jwt = new Mock<IJwtService>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("tok", DateTime.UtcNow));
        var sut = CreateSut(db, jwt);

        var res = await sut.UpdateProfileAsync(user.Id, new UpdateProfileRequest
        {
            Username = "newname",
            Email = "mail@example.com",
            IsProfilePublic = false
        });

        Assert.True(res.Success);
        Assert.Equal("tok", res.NewToken);
        jwt.Verify(j => j.GenerateToken(It.Is<User>(u => u.Username == "newname")), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ReturnsFailure()
    {
        await using var db = CreateContext();
        var sut = CreateSut(db);

        var res = await sut.ChangePasswordAsync(1, new ChangePasswordRequest
        {
            CurrentPassword = ValidPassword,
            NewPassword = NewValidPassword,
            ConfirmNewPassword = NewValidPassword
        });

        Assert.False(res.Success);
        Assert.Equal("Пользователь не найден", res.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsFailure()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "u", "u@example.com");
        var sut = CreateSut(db);

        var res = await sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
        {
            CurrentPassword = "wrongPw1",
            NewPassword = NewValidPassword,
            ConfirmNewPassword = NewValidPassword
        });

        Assert.False(res.Success);
        Assert.Equal("Неверный текущий пароль", res.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_Success_UpdatesStoredHash()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "u", "u@example.com");
        var oldHash = (byte[])user.PasswordHash.Clone();
        var sut = CreateSut(db);

        var res = await sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
        {
            CurrentPassword = ValidPassword,
            NewPassword = NewValidPassword,
            ConfirmNewPassword = NewValidPassword
        });

        Assert.True(res.Success);
        await db.Entry(user).ReloadAsync();
        Assert.NotEqual(oldHash, user.PasswordHash);
        Assert.True(JwtService.VerifyPasswordHash(NewValidPassword, user.PasswordHash, user.PasswordSalt));
    }

    [Fact]
    public async Task ChangePasswordAsync_SecondAttemptWithinHour_IsRateLimited()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "u", "u@example.com");
        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = CreateSut(db, cache: cache);

        var req = new ChangePasswordRequest
        {
            CurrentPassword = ValidPassword,
            NewPassword = NewValidPassword,
            ConfirmNewPassword = NewValidPassword
        };
        var first = await sut.ChangePasswordAsync(user.Id, req);
        Assert.True(first.Success);

        var second = await sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
        {
            CurrentPassword = NewValidPassword,
            NewPassword = "aaBb33",
            ConfirmNewPassword = "aaBb33"
        });

        Assert.False(second.Success);
        Assert.Contains("час", second.Message, StringComparison.Ordinal);
        Assert.NotNull(second.RetryAfterSeconds);
        Assert.True(second.RetryAfterSeconds >= 1);
    }

    [Fact]
    public async Task ChangePasswordAsync_AfterCooldownWindow_AllowsNextChange()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "u", "u@example.com");
        var cache = new MemoryCache(new MemoryCacheOptions());
        var key = "profile_pwd_change_" + user.Id;
        cache.Set(key, DateTime.UtcNow.AddHours(-2));

        var sut = CreateSut(db, cache: cache);

        var res = await sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
        {
            CurrentPassword = ValidPassword,
            NewPassword = NewValidPassword,
            ConfirmNewPassword = NewValidPassword
        });

        Assert.True(res.Success);
    }

    [Fact]
    public async Task GetPublicProfileAsync_UserMissing_ReturnsNull()
    {
        await using var db = CreateContext();
        var humidor = new Mock<IHumidorService>(MockBehavior.Strict);
        var sut = CreateSut(db, humidor: humidor);

        var dto = await sut.GetPublicProfileAsync("nobody");

        Assert.Null(dto);
        humidor.Verify(h => h.GetUserHumidors(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPublicProfileAsync_ProfilePrivate_ReturnsNull()
    {
        await using var db = CreateContext();
        await SeedUserAsync(db, "private", "p@example.com", isProfilePublic: false);
        var humidor = new Mock<IHumidorService>(MockBehavior.Strict);
        var sut = CreateSut(db, humidor: humidor);

        var dto = await sut.GetPublicProfileAsync("private");

        Assert.Null(dto);
        humidor.Verify(h => h.GetUserHumidors(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPublicProfileAsync_Public_ReturnsHumidorsFromService()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "pub", "pub@example.com", isProfilePublic: true);
        var list = new List<HumidorResponseDto>
        {
            new() { Id = 1, Name = "Main", Capacity = 50, CurrentCount = 0, CreatedAt = DateTime.UtcNow }
        };
        var humidor = new Mock<IHumidorService>();
        humidor.Setup(h => h.GetUserHumidors(user.Id)).ReturnsAsync(list);
        var sut = CreateSut(db, humidor: humidor);

        var dto = await sut.GetPublicProfileAsync("pub");

        Assert.NotNull(dto);
        Assert.Equal("pub", dto!.Username);
        Assert.Single(dto.Humidors);
        Assert.Equal("Main", dto.Humidors[0].Name);
        humidor.Verify(h => h.GetUserHumidors(user.Id), Times.Once);
    }

    [Fact]
    public async Task GetPublicHumidorAsync_UserMissing_ReturnsNull()
    {
        await using var db = CreateContext();
        var humidor = new Mock<IHumidorService>(MockBehavior.Strict);
        var sut = CreateSut(db, humidor: humidor);

        var dto = await sut.GetPublicHumidorAsync("ghost", 1);

        Assert.Null(dto);
        humidor.Verify(h => h.GetHumidorById(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPublicHumidorAsync_ProfilePrivate_ReturnsNull()
    {
        await using var db = CreateContext();
        await SeedUserAsync(db, "priv", "p@example.com", isProfilePublic: false);
        var humidor = new Mock<IHumidorService>(MockBehavior.Strict);
        var sut = CreateSut(db, humidor: humidor);

        var dto = await sut.GetPublicHumidorAsync("priv", 10);

        Assert.Null(dto);
        humidor.Verify(h => h.GetHumidorById(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPublicHumidorAsync_Public_DelegatesToHumidorService()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db, "pub", "pub@example.com", isProfilePublic: true);
        var detail = new HumidorDetailResponseDto { Id = 5, Name = "H", Capacity = 20, CreatedAt = DateTime.UtcNow };
        var humidor = new Mock<IHumidorService>();
        humidor.Setup(h => h.GetHumidorById(5, user.Id)).ReturnsAsync(detail);
        var sut = CreateSut(db, humidor: humidor);

        var result = await sut.GetPublicHumidorAsync("pub", 5);

        Assert.NotNull(result);
        Assert.Equal("H", result!.Name);
        humidor.Verify(h => h.GetHumidorById(5, user.Id), Times.Once);
    }
}
