using CigarHelper.Api.Models;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// Risk-based: пагинация и лимиты (анти-N+1 на уровне API — список через проекцию),
/// поиск по username/email, правила смены роли (последний админ, самоподтверждение, JWT только для себя).
/// </summary>
public class AdminUserServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AdminUsers_{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }

    private static string Unique(string prefix)
    {
        var s = $"{prefix}_{Guid.NewGuid():N}";
        return s.Length > 50 ? s[..50] : s;
    }

    private static async Task<User> SeedUserAsync(AppDbContext db, string usernamePrefix, Role role, string? email = null)
    {
        JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
        var u = new User
        {
            Username = Unique(usernamePrefix),
            Email = email ?? $"{Unique("e")}@test.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            Role = role
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return u;
    }

    /// <summary>Точное имя пользователя (для проверки сортировки по Username).</summary>
    private static async Task<User> SeedUserFixedUsernameAsync(AppDbContext db, string username, Role role, string? email = null)
    {
        JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
        var u = new User
        {
            Username = username,
            Email = email ?? $"{username.Replace(' ', '_')}@test.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            Role = role
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return u;
    }

    private static AdminUserService Sut(AppDbContext db, Mock<IJwtService>? jwt = null)
    {
        jwt ??= new Mock<IJwtService>();
        return new AdminUserService(db, jwt.Object);
    }

    [Fact]
    public async Task GetUsersAsync_EmptyDatabase_ReturnsEmptyPageWithZeroTotal()
    {
        await using var db = CreateContext();
        var sut = Sut(db);

        var res = await sut.GetUsersAsync(page: 1, pageSize: 20, search: null);

        Assert.Empty(res.Items);
        Assert.Equal(0, res.TotalCount);
        Assert.Equal(1, res.Page);
        Assert.Equal(20, res.PageSize);
    }

    [Fact]
    public async Task GetUsersAsync_NormalizesPageAndPageSize()
    {
        await using var db = CreateContext();
        await SeedUserAsync(db, "only", Role.User);
        var sut = Sut(db);

        var res = await sut.GetUsersAsync(page: -3, pageSize: 500, search: null);

        Assert.Equal(1, res.Page);
        Assert.Equal(100, res.PageSize);
        Assert.Single(res.Items);
    }

    [Fact]
    public async Task GetUsersAsync_PageSizeBelowOne_ClampsToOne()
    {
        await using var db = CreateContext();
        await SeedUserAsync(db, "a", Role.User);
        var sut = Sut(db);

        var res = await sut.GetUsersAsync(page: 1, pageSize: 0, search: null);

        Assert.Equal(1, res.PageSize);
        Assert.Single(res.Items);
    }

    [Fact]
    public async Task GetUsersAsync_OrderedByUsername_WithPagination()
    {
        await using var db = CreateContext();
        await SeedUserFixedUsernameAsync(db, "pg_charlie", Role.User);
        await SeedUserFixedUsernameAsync(db, "pg_alpha", Role.User);
        await SeedUserFixedUsernameAsync(db, "pg_bravo", Role.User);
        var sut = Sut(db);

        var page1 = await sut.GetUsersAsync(1, 2, null);
        var page2 = await sut.GetUsersAsync(2, 2, null);

        Assert.Equal(3, page1.TotalCount);
        Assert.Equal("pg_alpha", page1.Items[0].Username);
        Assert.Equal("pg_bravo", page1.Items[1].Username);
        Assert.Single(page2.Items);
        Assert.Equal("pg_charlie", page2.Items[0].Username);
    }

    [Fact]
    public async Task GetUsersAsync_SearchMatchesUsernameOrEmail_TrimsTerm()
    {
        await using var db = CreateContext();
        await SeedUserAsync(db, "other", Role.User);
        var match = await SeedUserAsync(db, "findme_x", Role.Moderator, email: "keeper@unit.test");
        var sut = Sut(db);

        var byName = await sut.GetUsersAsync(1, 50, "  findme_x  ");
        var byEmail = await sut.GetUsersAsync(1, 50, "unit.test");

        Assert.Single(byName.Items);
        Assert.Equal(match.Id, byName.Items[0].Id);
        Assert.Single(byEmail.Items);
        Assert.Equal(match.Id, byEmail.Items[0].Id);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_TargetNotFound_ReturnsFailure()
    {
        await using var db = CreateContext();
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = Sut(db, jwt);

        var res = await sut.UpdateUserRoleAsync(actorUserId: 1, targetUserId: 9999, newRole: Role.Admin, confirmSelfChange: false);

        Assert.False(res.Success);
        Assert.Equal("Пользователь не найден", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_SameRole_ReturnsSuccessWithoutPersistingChange()
    {
        await using var db = CreateContext();
        var u = await SeedUserAsync(db, "samerole", Role.User);
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = Sut(db, jwt);

        var res = await sut.UpdateUserRoleAsync(u.Id, u.Id, Role.User, confirmSelfChange: false);

        Assert.True(res.Success);
        Assert.Equal("Роль без изменений", res.Message);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_SelfChangeWithoutConfirm_ReturnsFailure()
    {
        await using var db = CreateContext();
        var u = await SeedUserAsync(db, "self", Role.User);
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = Sut(db, jwt);

        var res = await sut.UpdateUserRoleAsync(u.Id, u.Id, Role.Moderator, confirmSelfChange: false);

        Assert.False(res.Success);
        Assert.Contains("подтверждение", res.Message, StringComparison.OrdinalIgnoreCase);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_SelfChangeWithConfirm_IssuesNewToken()
    {
        await using var db = CreateContext();
        var u = await SeedUserAsync(db, "me", Role.User);
        var jwt = new Mock<IJwtService>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-for-me");
        var sut = Sut(db, jwt);

        var res = await sut.UpdateUserRoleAsync(u.Id, u.Id, Role.Moderator, confirmSelfChange: true);

        Assert.True(res.Success);
        Assert.Equal("jwt-for-me", res.NewToken);
        await db.Entry(u).ReloadAsync();
        Assert.Equal(Role.Moderator, u.Role);
        jwt.Verify(j => j.GenerateToken(It.Is<User>(x => x.Id == u.Id && x.Role == Role.Moderator)), Times.Once);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_CannotDemoteLastAdmin()
    {
        await using var db = CreateContext();
        var sole = await SeedUserAsync(db, "soleadmin", Role.Admin);
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = Sut(db, jwt);

        var res = await sut.UpdateUserRoleAsync(sole.Id, sole.Id, Role.User, confirmSelfChange: true);

        Assert.False(res.Success);
        Assert.Contains("последнего администратора", res.Message, StringComparison.OrdinalIgnoreCase);
        await db.Entry(sole).ReloadAsync();
        Assert.Equal(Role.Admin, sole.Role);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_TwoAdmins_OneCanBeDemoted()
    {
        await using var db = CreateContext();
        var keep = await SeedUserAsync(db, "keep_admin", Role.Admin);
        var drop = await SeedUserAsync(db, "drop_admin", Role.Admin);
        var jwt = new Mock<IJwtService>();
        var sut = Sut(db, jwt);

        var res = await sut.UpdateUserRoleAsync(keep.Id, drop.Id, Role.User, confirmSelfChange: false);

        Assert.True(res.Success);
        Assert.Null(res.NewToken);
        await db.Entry(drop).ReloadAsync();
        Assert.Equal(Role.User, drop.Role);
        jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_OtherUser_NoToken_NotSelf()
    {
        await using var db = CreateContext();
        var actor = await SeedUserAsync(db, "act", Role.Admin);
        var target = await SeedUserAsync(db, "tgt", Role.User);
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = Sut(db, jwt);

        var res = await sut.UpdateUserRoleAsync(actor.Id, target.Id, Role.Moderator, confirmSelfChange: false);

        Assert.True(res.Success);
        Assert.Null(res.NewToken);
        await db.Entry(target).ReloadAsync();
        Assert.Equal(Role.Moderator, target.Role);
    }
}
