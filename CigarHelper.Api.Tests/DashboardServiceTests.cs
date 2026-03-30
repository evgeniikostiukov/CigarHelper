using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// Тесты дашборда: агрегаты по коллекции, разрез по брендам, недавние обзоры.
/// </summary>
public class DashboardServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Dashboard_{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<User> SeedUserAsync(AppDbContext db)
    {
        JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
        var user = new User
        {
            Username = $"u_{Guid.NewGuid():N}"[..16],
            Email = $"e_{Guid.NewGuid():N}@test.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            Role = Role.User
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task<(Brand Brand, CigarBase Base)> SeedBrandAndCigarBaseAsync(AppDbContext db, string suffix)
    {
        var brand = new Brand
        {
            Name = $"Brand_{suffix}",
            CreatedAt = DateTime.UtcNow
        };
        db.Brands.Add(brand);
        await db.SaveChangesAsync();

        var cb = new CigarBase
        {
            Name = $"Cigar_{suffix}",
            BrandId = brand.Id,
            CreatedAt = DateTime.UtcNow,
            Strength = "Medium",
            Size = "Robusto"
        };
        db.CigarBases.Add(cb);
        await db.SaveChangesAsync();

        return (brand, cb);
    }

    private static async Task<Humidor> SeedHumidorAsync(AppDbContext db, int userId, int capacity)
    {
        var h = new Humidor
        {
            UserId = userId,
            Name = $"H_{Guid.NewGuid():N}"[..16],
            Capacity = capacity,
            CreatedAt = DateTime.UtcNow
        };
        db.Humidors.Add(h);
        await db.SaveChangesAsync();
        return h;
    }

    private static async Task<UserCigar> SeedUserCigarAsync(AppDbContext db, int userId, int cigarBaseId, int? humidorId, int? rating = null)
    {
        var uc = new UserCigar
        {
            UserId = userId,
            CigarBaseId = cigarBaseId,
            HumidorId = humidorId,
            Price = 10m,
            Rating = rating,
            CreatedAt = DateTime.UtcNow
        };
        db.UserCigars.Add(uc);
        await db.SaveChangesAsync();
        return uc;
    }

    private static async Task<Review> SeedReviewAsync(AppDbContext db, int userId, int userCigarId, DateTime createdAt)
    {
        var review = new Review
        {
            UserId = userId,
            CigarId = userCigarId,
            Title = $"Review_{Guid.NewGuid():N}"[..12],
            Content = "Test content",
            Rating = 8,
            CreatedAt = createdAt
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        return review;
    }

    [Fact]
    public async Task GetUserDashboardSummaryAsync_NoData_ReturnsZeros()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = new DashboardService(db);

        var summary = await sut.GetUserDashboardSummaryAsync(user.Id);

        Assert.Equal(0, summary.TotalHumidors);
        Assert.Equal(0, summary.TotalCigars);
        Assert.Equal(0, summary.TotalCapacity);
        Assert.Equal(0, summary.AverageFillPercent);
        Assert.Empty(summary.BrandBreakdown);
        Assert.Empty(summary.RecentReviews);
    }

    [Fact]
    public async Task GetUserDashboardSummaryAsync_ComputesAggregatesAndBrandBreakdown()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var other = await SeedUserAsync(db);
        var (brand1, cb1) = await SeedBrandAndCigarBaseAsync(db, "A");
        var (brand2, cb2) = await SeedBrandAndCigarBaseAsync(db, "B");

        var h1 = await SeedHumidorAsync(db, user.Id, capacity: 10);
        var h2 = await SeedHumidorAsync(db, user.Id, capacity: 20);

        // Пользовательские сигары (часть в хьюмидорах, часть "на полке")
        await SeedUserCigarAsync(db, user.Id, cb1.Id, h1.Id, rating: 7);
        await SeedUserCigarAsync(db, user.Id, cb1.Id, h1.Id, rating: 9);
        await SeedUserCigarAsync(db, user.Id, cb2.Id, h2.Id, rating: null);
        await SeedUserCigarAsync(db, user.Id, cb2.Id, null, rating: 5);

        // Для другого пользователя создаём шум
        var otherHumidor = await SeedHumidorAsync(db, other.Id, capacity: 5);
        await SeedUserCigarAsync(db, other.Id, cb1.Id, otherHumidor.Id, rating: 10);

        var sut = new DashboardService(db);

        var summary = await sut.GetUserDashboardSummaryAsync(user.Id);

        Assert.Equal(2, summary.TotalHumidors);
        Assert.Equal(4, summary.TotalCigars);
        Assert.Equal(30, summary.TotalCapacity);
        Assert.InRange(summary.AverageFillPercent, 0, 100);

        Assert.Equal(2, summary.BrandBreakdown.Count);
        var brand1Item = summary.BrandBreakdown.Single(b => b.BrandId == brand1.Id);
        var brand2Item = summary.BrandBreakdown.Single(b => b.BrandId == brand2.Id);

        Assert.Equal(2, brand1Item.CigarCount);
        Assert.Equal(2, brand2Item.CigarCount);
        Assert.Equal(8, brand1Item.AverageRating);
        Assert.Equal(5, brand2Item.AverageRating);
    }

    [Fact]
    public async Task GetUserDashboardSummaryAsync_IncludesRecentReviewsOnlyForUser_Top5()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var other = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db, "R");

        var h1 = await SeedHumidorAsync(db, user.Id, capacity: 10);
        var userCigar = await SeedUserCigarAsync(db, user.Id, cb.Id, h1.Id, rating: 8);
        var otherCigar = await SeedUserCigarAsync(db, other.Id, cb.Id, null, rating: 9);

        // 6 обзоров пользователя, берём последние 5
        for (var i = 0; i < 6; i++)
        {
            await SeedReviewAsync(db, user.Id, userCigar.Id, DateTime.UtcNow.AddMinutes(-i));
        }

        // Обзор другого пользователя не должен попасть в выборку
        await SeedReviewAsync(db, other.Id, otherCigar.Id, DateTime.UtcNow);

        var sut = new DashboardService(db);

        var summary = await sut.GetUserDashboardSummaryAsync(user.Id);

        Assert.Equal(5, summary.RecentReviews.Count);
        Assert.All(summary.RecentReviews, rr => Assert.Equal(user.Id, db.Reviews.Single(r => r.Id == rr.Id).UserId));

        var ordered = summary.RecentReviews
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => r.Id)
            .ToList();

        Assert.Equal(ordered, summary.RecentReviews.Select(r => r.Id).ToList());
    }
}

