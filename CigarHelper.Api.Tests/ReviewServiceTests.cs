using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// Risk-based: фильтры списка, усечение summary, владение при правках, FK на UserCigar при создании,
/// каскад по изображениям при удалении. Без сети — Create/Update без URL картинок (Images пусты).
/// Соответствует слою сервиса fullstack; для Postgres в бою важны индексы по UserId/CigarId и каскады — см. модель.
/// </summary>
public class ReviewServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Review_{Guid.NewGuid():N}")
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
            CreatedAt = DateTime.UtcNow
        };
        db.CigarBases.Add(cb);
        await db.SaveChangesAsync();
        return (brand, cb);
    }

    private static async Task<UserCigar> SeedUserCigarAsync(AppDbContext db, int userId, int cigarBaseId)
    {
        var uc = new UserCigar
        {
            UserId = userId,
            CigarBaseId = cigarBaseId,
            HumidorId = null,
            CreatedAt = DateTime.UtcNow
        };
        db.UserCigars.Add(uc);
        await db.SaveChangesAsync();
        return uc;
    }

    private static ReviewService Sut(AppDbContext db) => new(db);

    private static CreateReviewRequest BuildCreateRequest(
        int cigarBaseId,
        string title,
        string content,
        int rating = 8,
        int? userCigarId = null) =>
        new()
        {
            Title = title,
            Content = content,
            Rating = rating,
            CigarBaseId = cigarBaseId,
            UserCigarId = userCigarId,
            Images = new List<CreateReviewImageRequest>()
        };

    private static async Task<Review> SeedReviewAsync(
        AppDbContext db,
        int userId,
        int cigarBaseId,
        int? userCigarId,
        string title,
        string content,
        DateTime createdAtUtc,
        int rating = 7)
    {
        var r = new Review
        {
            Title = title,
            Content = content,
            Rating = rating,
            UserId = userId,
            CigarBaseId = cigarBaseId,
            CigarId = userCigarId,
            SmokingDate = DateTime.UtcNow,
            CreatedAt = createdAtUtc
        };
        db.Reviews.Add(r);
        await db.SaveChangesAsync();
        return r;
    }

    [Fact]
    public async Task GetReviewsAsync_NoFilters_ReturnsOrderedByCreatedAtDescending()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar1 = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var cigar2 = await SeedUserCigarAsync(db, user.Id, cb.Id);
        await SeedReviewAsync(db, user.Id, cb.Id, cigar1.Id, "Old", "a", DateTime.UtcNow.AddDays(-3));
        await SeedReviewAsync(db, user.Id, cb.Id, cigar2.Id, "New", "b", DateTime.UtcNow.AddDays(-1));
        var sut = Sut(db);

        var list = await sut.GetReviewsAsync();

        Assert.Equal(2, list.Count);
        Assert.Equal("New", list[0].Title);
        Assert.Equal("Old", list[1].Title);
    }

    [Fact]
    public async Task GetReviewsAsync_FilterByUserId_ReturnsOnlyThatUser()
    {
        await using var db = CreateContext();
        var u1 = await SeedUserAsync(db);
        var u2 = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var c1 = await SeedUserCigarAsync(db, u1.Id, cb.Id);
        var c2 = await SeedUserCigarAsync(db, u2.Id, cb.Id);
        await SeedReviewAsync(db, u1.Id, cb.Id, c1.Id, "A", "x", DateTime.UtcNow);
        await SeedReviewAsync(db, u2.Id, cb.Id, c2.Id, "B", "y", DateTime.UtcNow);
        var sut = Sut(db);

        var list = await sut.GetReviewsAsync(userId: u1.Id);

        Assert.Single(list);
        Assert.Equal("A", list[0].Title);
    }

    [Fact]
    public async Task GetReviewsAsync_FilterByUserCigarId_ReturnsOnlyThatCollectionRow()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var c1 = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var c2 = await SeedUserCigarAsync(db, user.Id, cb.Id);
        await SeedReviewAsync(db, user.Id, cb.Id, c1.Id, "On1", "x", DateTime.UtcNow);
        await SeedReviewAsync(db, user.Id, cb.Id, c2.Id, "On2", "y", DateTime.UtcNow);
        var sut = Sut(db);

        var list = await sut.GetReviewsAsync(userCigarId: c1.Id);

        Assert.Single(list);
        Assert.Equal("On1", list[0].Title);
    }

    [Fact]
    public async Task GetReviewsAsync_ContentLongerThan200_SummaryIsTruncatedWithEllipsis()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var longBody = new string('x', 250);
        await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "T", longBody, DateTime.UtcNow);
        var sut = Sut(db);

        var list = await sut.GetReviewsAsync();

        Assert.Single(list);
        Assert.Equal(200, list[0].Summary.Length);
        Assert.EndsWith("...", list[0].Summary, StringComparison.Ordinal);
        Assert.Equal(197, list[0].Summary.Count(c => c == 'x'));
    }

    [Fact]
    public async Task GetReviewsAsync_WithImages_SetsMainImageAndCount()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Pic", "body", DateTime.UtcNow);
        db.ReviewImages.Add(new ReviewImage
        {
            ReviewId = rev.Id,
            ImageBytes = new byte[] { 0xFF, 0xD8, 0xFF },
            Caption = "main",
            CreatedAt = DateTime.UtcNow
        });
        db.ReviewImages.Add(new ReviewImage
        {
            ReviewId = rev.Id,
            ImageBytes = new byte[] { 1, 2, 3 },
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var list = await sut.GetReviewsAsync();

        Assert.Equal(2, list[0].ImageCount);
        Assert.NotNull(list[0].MainImageBytes);
    }

    [Fact]
    public async Task GetReviewByIdAsync_NotFound_ReturnsNull()
    {
        await using var db = CreateContext();
        var sut = Sut(db);

        var dto = await sut.GetReviewByIdAsync(99999);

        Assert.Null(dto);
    }

    [Fact]
    public async Task GetReviewByIdAsync_Found_ReturnsDtoWithCigarAndUser()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (brand, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Full", "Text here", DateTime.UtcNow, rating: 9);
        var sut = Sut(db);

        var dto = await sut.GetReviewByIdAsync(rev.Id);

        Assert.NotNull(dto);
        Assert.Equal("Full", dto!.Title);
        Assert.Equal(9, dto.Rating);
        Assert.Equal(user.Username, dto.Username);
        Assert.Equal(cb.Name, dto.CigarName);
        Assert.Equal(brand.Name, dto.CigarBrand);
    }

    [Fact]
    public async Task CreateReviewAsync_CigarBaseMissing_ThrowsNotFoundException()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = Sut(db);

        var ex = await Assert.ThrowsAsync<CigarHelper.Api.Exceptions.NotFoundException>(() =>
            sut.CreateReviewAsync(user.Id, BuildCreateRequest(404, "Tit", "Content here long enough")));

        Assert.Contains("404", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateReviewAsync_ValidRequest_OnlyCigarBase_PersistsWithoutUserCigar()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var sut = Sut(db);

        var dto = await sut.CreateReviewAsync(user.Id, BuildCreateRequest(cb.Id, "Nice", "Good smoke", rating: 6));

        Assert.True(dto.Id > 0);
        Assert.Equal("Nice", dto.Title);
        Assert.Equal(6, dto.Rating);
        Assert.Equal(user.Id, dto.UserId);
        Assert.Equal(cb.Id, dto.CigarBaseId);
        Assert.Null(dto.UserCigarId);
        var row = await db.Reviews.SingleAsync(r => r.Id == dto.Id);
        Assert.Equal(cb.Id, row.CigarBaseId);
        Assert.Null(row.CigarId);
    }

    [Fact]
    public async Task CreateReviewAsync_WithUserCigarId_LinksCollectionRow()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var sut = Sut(db);

        var dto = await sut.CreateReviewAsync(
            user.Id,
            BuildCreateRequest(cb.Id, "Linked", "Body text here", rating: 7, userCigarId: cigar.Id));

        Assert.Equal(cb.Id, dto.CigarBaseId);
        Assert.Equal(cigar.Id, dto.UserCigarId);
        var row = await db.Reviews.SingleAsync(r => r.Id == dto.Id);
        Assert.Equal(cigar.Id, row.CigarId);
    }

    [Fact]
    public async Task UpdateReviewAsync_NotFound_ReturnsNull()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = Sut(db);

        var dto = await sut.UpdateReviewAsync(999, user.Id, new UpdateReviewRequest
        {
            Title = "abc",
            Content = "something",
            Rating = 5
        });

        Assert.Null(dto);
    }

    [Fact]
    public async Task UpdateReviewAsync_OtherUser_ThrowsUnauthorizedAccessException()
    {
        await using var db = CreateContext();
        var owner = await SeedUserAsync(db);
        var intruder = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, owner.Id, cb.Id);
        var rev = await SeedReviewAsync(db, owner.Id, cb.Id, cigar.Id, "Mine", "x", DateTime.UtcNow);
        var sut = Sut(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.UpdateReviewAsync(rev.Id, intruder.Id, new UpdateReviewRequest
            {
                Title = "hack",
                Content = "no",
                Rating = 1
            }));
    }

    [Fact]
    public async Task UpdateReviewAsync_Owner_UpdatesFields()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Was", "old", DateTime.UtcNow.AddDays(-2), rating: 5);
        var sut = Sut(db);

        var dto = await sut.UpdateReviewAsync(rev.Id, user.Id, new UpdateReviewRequest
        {
            Title = "Now",
            Content = "new",
            Rating = 10,
            Venue = "Home"
        });

        Assert.NotNull(dto);
        Assert.Equal("Now", dto!.Title);
        Assert.Equal("new", dto.Content);
        Assert.Equal(10, dto.Rating);
        Assert.Equal("Home", dto.Venue);
        Assert.NotNull(dto.UpdatedAt);
    }

    [Fact]
    public async Task UpdateReviewAsync_RemovesListedImages()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Imgs", "x", DateTime.UtcNow);
        var img1 = new ReviewImage { ReviewId = rev.Id, ImageBytes = new byte[] { 1 }, CreatedAt = DateTime.UtcNow };
        var img2 = new ReviewImage { ReviewId = rev.Id, ImageBytes = new byte[] { 2 }, CreatedAt = DateTime.UtcNow };
        db.ReviewImages.AddRange(img1, img2);
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var dto = await sut.UpdateReviewAsync(rev.Id, user.Id, new UpdateReviewRequest
        {
            Title = "Imgs",
            Content = "x2",
            Rating = 7,
            ImageIdsToRemove = new List<int> { img1.Id }
        });

        Assert.NotNull(dto);
        Assert.Single(dto!.Images);
        Assert.DoesNotContain(dto.Images, i => i.Id == img1.Id);
        Assert.False(await db.ReviewImages.AnyAsync(i => i.Id == img1.Id));
    }

    [Fact]
    public async Task DeleteReviewAsync_NotFound_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var sut = Sut(db);

        var ok = await sut.DeleteReviewAsync(12345, user.Id);

        Assert.False(ok);
    }

    [Fact]
    public async Task DeleteReviewAsync_OtherUser_ThrowsUnauthorizedAccessException()
    {
        await using var db = CreateContext();
        var owner = await SeedUserAsync(db);
        var other = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, owner.Id, cb.Id);
        var rev = await SeedReviewAsync(db, owner.Id, cb.Id, cigar.Id, "X", "y", DateTime.UtcNow);
        var sut = Sut(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.DeleteReviewAsync(rev.Id, other.Id));
    }

    [Fact]
    public async Task DeleteReviewAsync_Owner_SoftDeletes_KeepsRowAndImages()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Del", "z", DateTime.UtcNow);
        db.ReviewImages.Add(new ReviewImage
        {
            ReviewId = rev.Id,
            ImageBytes = new byte[] { 9 },
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var ok = await sut.DeleteReviewAsync(rev.Id, user.Id);

        Assert.True(ok);
        var row = await db.Reviews.AsNoTracking().SingleAsync(r => r.Id == rev.Id);
        Assert.NotNull(row.DeletedAt);
        Assert.True(await db.ReviewImages.AnyAsync(i => i.ReviewId == rev.Id));
    }

    [Fact]
    public async Task DeleteReviewAsync_Owner_AlreadySoftDeleted_ReturnsTrue()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "X", "y", DateTime.UtcNow);
        rev.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var ok = await sut.DeleteReviewAsync(rev.Id, user.Id);

        Assert.True(ok);
    }

    [Fact]
    public async Task GetReviewsAsync_ExcludesSoftDeleted()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Active", "a", DateTime.UtcNow);
        var gone = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Hidden", "b", DateTime.UtcNow.AddMinutes(-1));
        gone.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var list = await sut.GetReviewsAsync();

        Assert.Single(list);
        Assert.Equal("Active", list[0].Title);
    }

    [Fact]
    public async Task GetReviewByIdAsync_SoftDeleted_ReturnsNull()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Gone", "x", DateTime.UtcNow);
        rev.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var dto = await sut.GetReviewByIdAsync(rev.Id);

        Assert.Null(dto);
    }

    [Fact]
    public async Task UpdateReviewAsync_SoftDeleted_ReturnsNull()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Old", "x", DateTime.UtcNow);
        rev.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var dto = await sut.UpdateReviewAsync(rev.Id, user.Id, new UpdateReviewRequest
        {
            Title = "New",
            Content = "y",
            Rating = 5
        });

        Assert.Null(dto);
    }

    [Fact]
    public async Task GetDeletedReviewsForStaffAsync_ReturnsOnlyDeleted_OrderedByDeletedAtDesc()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Live", "a", DateTime.UtcNow);
        var d1 = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "D1", "b", DateTime.UtcNow.AddHours(-2));
        d1.DeletedAt = DateTime.UtcNow.AddHours(-2);
        var d2 = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "D2", "c", DateTime.UtcNow.AddHours(-3));
        d2.DeletedAt = DateTime.UtcNow.AddHours(-1);
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var page = await sut.GetDeletedReviewsForStaffAsync(1, 20);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(new[] { d2.Id, d1.Id }, page.Items.Select(x => x.Id).ToArray());
    }

    [Fact]
    public async Task RestoreReviewByStaffAsync_WhenDeleted_ClearsDeletedAt_AndReviewReappearsInPublicList()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Back", "z", DateTime.UtcNow);
        rev.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var sut = Sut(db);

        var ok = await sut.RestoreReviewByStaffAsync(rev.Id);

        Assert.True(ok);
        Assert.Null((await db.Reviews.AsNoTracking().SingleAsync(r => r.Id == rev.Id)).DeletedAt);
        var list = await sut.GetReviewsAsync();
        Assert.Contains(list, x => x.Id == rev.Id);
    }

    [Fact]
    public async Task RestoreReviewByStaffAsync_NotDeleted_ReturnsFalse()
    {
        await using var db = CreateContext();
        var user = await SeedUserAsync(db);
        var (_, cb) = await SeedBrandAndCigarBaseAsync(db);
        var cigar = await SeedUserCigarAsync(db, user.Id, cb.Id);
        var rev = await SeedReviewAsync(db, user.Id, cb.Id, cigar.Id, "Live", "z", DateTime.UtcNow);
        var sut = Sut(db);

        var ok = await sut.RestoreReviewByStaffAsync(rev.Id);

        Assert.False(ok);
    }
}
