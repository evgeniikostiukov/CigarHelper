using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// Стратегия (risk-based): изоляция по userId, границы ёмкости, идемпотентность добавления,
/// целостность связей сигара–хьюмидор. InMemory EF без моков — проверяем реальные запросы.
/// </summary>
public class HumidorServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Humidor_{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }

    private static string Unique(string prefix)
    {
        var s = $"{prefix}_{Guid.NewGuid():N}";
        return s.Length > 50 ? s[..50] : s;
    }

    private static async Task<User> SeedUserAsync(AppDbContext db)
    {
        JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
        var u = new User
        {
            Username = Unique("u"),
            Email = $"{Unique("e")}@test.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            Role = Role.User
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return u;
    }

    private static async Task<(Brand Brand, CigarBase Base)> SeedBrandAndCigarBaseAsync(AppDbContext db)
    {
        var tag = Guid.NewGuid().ToString("N")[..10];
        var brand = new Brand { Name = $"B_{tag}", CreatedAt = DateTime.UtcNow };
        db.Brands.Add(brand);
        await db.SaveChangesAsync();
        var cb = new CigarBase
        {
            Name = $"C_{tag}",
            BrandId = brand.Id,
            CreatedAt = DateTime.UtcNow,
            Strength = "Mild",
            LengthMm = null,
            Diameter = null
        };
        db.CigarBases.Add(cb);
        await db.SaveChangesAsync();
        return (brand, cb);
    }

    private static async Task<Humidor> SeedHumidorAsync(AppDbContext db, int userId, int capacity, string? name = null)
    {
        var h = new Humidor
        {
            UserId = userId,
            Name = name ?? $"H_{Guid.NewGuid():N}",
            Capacity = capacity,
            CreatedAt = DateTime.UtcNow
        };
        db.Humidors.Add(h);
        await db.SaveChangesAsync();
        return h;
    }

    private static async Task<UserCigar> SeedUserCigarAsync(
        AppDbContext db,
        int userId,
        int cigarBaseId,
        int? humidorId,
        int quantity = 1)
    {
        var uc = new UserCigar
        {
            UserId = userId,
            CigarBaseId = cigarBaseId,
            HumidorId = humidorId,
            Price = 10m,
            Rating = 4,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };
        db.UserCigars.Add(uc);
        await db.SaveChangesAsync();
        return uc;
    }

    [Fact]
    public async Task GetUserHumidors_NoHumidors_ReturnsEmptyList()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = new HumidorService(db);

        var list = await sut.GetUserHumidors(user.Id);

        Assert.Empty(list);
    }

    [Fact]
    public async Task GetUserHumidors_ReturnsOnlyForUser_AndCurrentCountMatchesCigars()
    {
        await using var db = CreateContext();
        var u1 = await SeedUserAsync(db);
        var u2 = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h1 = await SeedHumidorAsync(db, u1.Id, 10);
        await SeedHumidorAsync(db, u2.Id, 5);
        await SeedUserCigarAsync(db, u1.Id, cb.Id, h1.Id, quantity: 3);
        await SeedUserCigarAsync(db, u1.Id, cb.Id, h1.Id, quantity: 2);
        var sut = new HumidorService(db);

        var forUser1 = await sut.GetUserHumidors(u1.Id);

        Assert.Single(forUser1);
        Assert.Equal(5, forUser1[0].CurrentCount);
        var forUser2 = await sut.GetUserHumidors(u2.Id);
        Assert.Single(forUser2);
        Assert.Equal(0, forUser2[0].CurrentCount);
    }

    [Fact]
    public async Task GetHumidorById_WrongUser_ReturnsNull()
    {
        await using var db = CreateContext();
        var owner = await SeedUserAsync(db);
        var other = await SeedUserAsync(db);
        var h = await SeedHumidorAsync(db, owner.Id, 5);
        var sut = new HumidorService(db);

        var dto = await sut.GetHumidorById(h.Id, other.Id);

        Assert.Null(dto);
    }

    [Fact]
    public async Task GetHumidorById_UnknownId_ReturnsNull()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = new HumidorService(db);

        var dto = await sut.GetHumidorById(99999, user.Id);

        Assert.Null(dto);
    }

    [Fact]
    public async Task GetHumidorById_ReturnsCigarsWithBrand()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (brand, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 5);
        await SeedUserCigarAsync(db, user.Id, cb.Id, h.Id);
        var sut = new HumidorService(db);

        var detail = await sut.GetHumidorById(h.Id, user.Id);

        Assert.NotNull(detail);
        Assert.Single(detail!.Cigars);
        Assert.Equal(cb.Name, detail.Cigars[0].Name);
        Assert.Equal(brand.Name, detail.Cigars[0].BrandName);
        Assert.Equal(brand.Name, detail.Cigars[0].Brand.Name);
    }

    [Fact]
    public async Task CreateHumidor_Persists_AndReturnsResponse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = new HumidorService(db);
        var dto = new HumidorCreateDto { Name = "Office", Description = "Desk", Capacity = 25 };

        var res = await sut.CreateHumidor(dto, user.Id);

        Assert.True(res.Id > 0);
        Assert.Equal("Office", res.Name);
        Assert.Equal(25, res.Capacity);
        Assert.Equal(0, res.CurrentCount);
        Assert.True(await db.Humidors.AnyAsync(x => x.Id == res.Id && x.UserId == user.Id));
    }

    [Fact]
    public async Task UpdateHumidor_NotFound_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = new HumidorService(db);

        var ok = await sut.UpdateHumidor(999, new HumidorUpdateDto { Name = "X", Capacity = 1 }, user.Id);

        Assert.False(ok);
    }

    [Fact]
    public async Task UpdateHumidor_Success_UpdatesFields()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 10, "Old");
        var sut = new HumidorService(db);
        var before = DateTime.UtcNow.AddMinutes(-1);

        var ok = await sut.UpdateHumidor(h.Id, new HumidorUpdateDto
        {
            Name = "NewName",
            Description = "D",
            Capacity = 20,
            Humidity = 65
        }, user.Id);

        Assert.True(ok);
        await db.Entry(h).ReloadAsync();
        Assert.Equal("NewName", h.Name);
        Assert.Equal(20, h.Capacity);
        Assert.Equal(65, h.Humidity);
        Assert.NotNull(h.UpdatedAt);
        Assert.True(h.UpdatedAt >= before);
    }

    [Fact]
    public async Task DeleteHumidor_NotFound_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = new HumidorService(db);

        var ok = await sut.DeleteHumidor(888, user.Id);

        Assert.False(ok);
    }

    [Fact]
    public async Task DeleteHumidor_RemovesHumidor()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 3);
        var sut = new HumidorService(db);

        var ok = await sut.DeleteHumidor(h.Id, user.Id);

        Assert.True(ok);
        Assert.False(await db.Humidors.AnyAsync(x => x.Id == h.Id));
    }

    [Fact]
    public async Task AddCigarToHumidor_HumidorMissing_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id, null);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(999, cigar.Id, user.Id);

        Assert.False(ok);
    }

    [Fact]
    public async Task AddCigarToHumidor_CigarMissing_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 5);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(h.Id, 99999, user.Id);

        Assert.False(ok);
    }

    [Fact]
    public async Task AddCigarToHumidor_CigarOwnedByAnotherUser_ReturnsFalse()
    {
        await using var db = CreateContext();
        var u1 = await SeedUserAsync(db);
        var u2 = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, u1.Id, 5);
        var cigar = await SeedUserCigarAsync(db, u2.Id, cb.Id, null);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(h.Id, cigar.Id, u1.Id);

        Assert.False(ok);
    }

    [Fact]
    public async Task AddCigarToHumidor_AlreadyInSameHumidor_ReturnsTrue_WithoutDoublingCount()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 5);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id, h.Id, quantity: 4);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(h.Id, cigar.Id, user.Id);

        Assert.True(ok);
        var list = await sut.GetUserHumidors(user.Id);
        Assert.Equal(4, list[0].CurrentCount);
    }

    [Fact]
    public async Task AddCigarToHumidor_CapacityFull_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 1);
        await SeedUserCigarAsync(db, user.Id, cb.Id, h.Id);
        var (_, cb2) = await SeedBrandAndCigarBaseAsync(db);
        var loose = await SeedUserCigarAsync(db, user.Id, cb2.Id, null);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(h.Id, loose.Id, user.Id);

        Assert.False(ok);
        Assert.Null((await db.UserCigars.FindAsync(loose.Id))!.HumidorId);
    }

    [Fact]
    public async Task AddCigarToHumidor_CapacityWouldBeExceededByQuantity_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb1) = await SeedBrandAndCigarBaseAsync(db);
        var (_, cb2) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, capacity: 10);

        await SeedUserCigarAsync(db, user.Id, cb1.Id, h.Id, quantity: 9);
        var incoming = await SeedUserCigarAsync(db, user.Id, cb2.Id, null, quantity: 2);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(h.Id, incoming.Id, user.Id);

        Assert.False(ok);
        Assert.Null((await db.UserCigars.FindAsync(incoming.Id))!.HumidorId);
        var list = await sut.GetUserHumidors(user.Id);
        Assert.Equal(9, list.Single().CurrentCount);
    }

    [Fact]
    public async Task AddCigarToHumidor_Success_AssignsHumidor()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 5);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id, null);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(h.Id, cigar.Id, user.Id);

        Assert.True(ok);
        await db.Entry(cigar).ReloadAsync();
        Assert.Equal(h.Id, cigar.HumidorId);
    }

    [Fact]
    public async Task AddCigarToHumidor_MovesFromOtherHumidor_WhenCapacityAllows()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h1 = await SeedHumidorAsync(db, user.Id, 5);
        var h2 = await SeedHumidorAsync(db, user.Id, 5);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id, h1.Id);
        var sut = new HumidorService(db);

        var ok = await sut.AddCigarToHumidor(h2.Id, cigar.Id, user.Id);

        Assert.True(ok);
        await db.Entry(cigar).ReloadAsync();
        Assert.Equal(h2.Id, cigar.HumidorId);
    }

    [Fact]
    public async Task RemoveCigarFromHumidor_NotInHumidor_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 5);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id, null);
        var sut = new HumidorService(db);

        var ok = await sut.RemoveCigarFromHumidor(h.Id, cigar.Id, user.Id);

        Assert.False(ok);
    }

    [Fact]
    public async Task RemoveCigarFromHumidor_Success_ClearsHumidorId()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 5);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id, h.Id);
        var sut = new HumidorService(db);

        var ok = await sut.RemoveCigarFromHumidor(h.Id, cigar.Id, user.Id);

        Assert.True(ok);
        await db.Entry(cigar).ReloadAsync();
        Assert.Null(cigar.HumidorId);
    }

    [Fact]
    public async Task GetCigarsInHumidor_WrongUser_ReturnsEmpty()
    {
        await using var db = CreateContext();
        var u1 = await SeedUserAsync(db);
        var u2 = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, u1.Id, 5);
        await SeedUserCigarAsync(db, u1.Id, cb.Id, h.Id);
        var sut = new HumidorService(db);

        var list = await sut.GetCigarsInHumidor(h.Id, u2.Id);

        Assert.Empty(list);
    }

    [Fact]
    public async Task GetCigarsInHumidor_ReturnsMappedCigarsWithBrand()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (brand, cb) = await SeedBrandAndCigarBaseAsync(db);
        var h = await SeedHumidorAsync(db, user.Id, 5);
        await SeedUserCigarAsync(db, user.Id, cb.Id, h.Id);
        var sut = new HumidorService(db);

        var list = await sut.GetCigarsInHumidor(h.Id, user.Id);

        Assert.Single(list);
        Assert.Equal(cb.Name, list[0].Name);
        Assert.Equal(brand.Name, list[0].Brand.Name);
    }
}
