using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CigarHelper.Api.Tests;

public class ReviewCommentsIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task GetReviewComments_InvalidReviewId_Returns400()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.GetAsync("/api/reviewcomments?reviewId=0");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task GetReviewComments_Anonymous_ActiveReview_ReturnsOk()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        int reviewId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            reviewId = await SeedMinimalReviewAsync(db);
        }

        using var client = factory.CreateClient();
        using var res = await client.GetAsync($"/api/reviewcomments?reviewId={reviewId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var list = await res.Content.ReadFromJsonAsync<List<ReviewCommentDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetReviewComments_DeletedReview_ReturnsEmpty()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        int reviewId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            reviewId = await SeedMinimalReviewAsync(db);
            var r = await db.Reviews.FirstAsync(x => x.Id == reviewId);
            r.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        using var res = await client.GetAsync($"/api/reviewcomments?reviewId={reviewId}");
        res.EnsureSuccessStatusCode();
        var list = await res.Content.ReadFromJsonAsync<List<ReviewCommentDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task PostReviewComment_ByUser_Pending_ThenModeratorApproves_AnonymousSeesComment()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        int reviewId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            reviewId = await SeedMinimalReviewAsync(db);
        }

        using var client = factory.CreateClient();
        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"ru{Guid.NewGuid():N}"[..11],
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = true,
        });
        registerRes.EnsureSuccessStatusCode();
        var authBody = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        using var post = await client.PostAsJsonAsync("/api/reviewcomments", new CreateReviewCommentRequest
        {
            ReviewId = reviewId,
            Body = "  Коммент к обзору  ",
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var created = await post.Content.ReadFromJsonAsync<ReviewCommentDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(CigarCommentModerationStatus.Pending, created.ModerationStatus);

        client.DefaultRequestHeaders.Authorization = null;
        using (var getEmpty = await client.GetAsync($"/api/reviewcomments?reviewId={reviewId}"))
        {
            getEmpty.EnsureSuccessStatusCode();
            var emptyList = await getEmpty.Content.ReadFromJsonAsync<List<ReviewCommentDto>>(JsonOptions);
            Assert.NotNull(emptyList);
            Assert.Empty(emptyList);
        }

        var modUsername = $"rm{Guid.NewGuid():N}"[..9];
        var modEmail = $"{Guid.NewGuid():N}@modrev.test";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
            db.Users.Add(new User
            {
                Username = modUsername,
                Email = modEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                Role = Role.Moderator,
            });
            await db.SaveChangesAsync();
        }

        using var loginMod = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Username = modUsername,
            Password = "abCd12",
        });
        loginMod.EnsureSuccessStatusCode();
        var modAuth = await loginMod.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(modAuth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", modAuth.Token);

        using var approve = await client.PostAsync(
            $"/api/admin/review-comments/{created.Id}/approve",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        using var get = await client.GetAsync($"/api/reviewcomments?reviewId={reviewId}");
        get.EnsureSuccessStatusCode();
        var list = await get.Content.ReadFromJsonAsync<List<ReviewCommentDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("Коммент к обзору", list[0].Body);
        Assert.Equal(CigarCommentModerationStatus.Approved, list[0].ModerationStatus);
    }

    [Fact]
    public async Task PostReviewComment_ByModerator_IsApprovedImmediately_OnPublicList()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        int reviewId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            reviewId = await SeedMinimalReviewAsync(db);
        }

        var modUsername = $"mr{Guid.NewGuid():N}"[..9];
        var modEmail = $"{Guid.NewGuid():N}@modrev2.test";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
            db.Users.Add(new User
            {
                Username = modUsername,
                Email = modEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                Role = Role.Moderator,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        using var loginMod = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Username = modUsername,
            Password = "abCd12",
        });
        loginMod.EnsureSuccessStatusCode();
        var modAuth = await loginMod.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(modAuth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", modAuth.Token);

        using var post = await client.PostAsJsonAsync("/api/reviewcomments", new CreateReviewCommentRequest
        {
            ReviewId = reviewId,
            Body = "От модератора к обзору",
        });
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<ReviewCommentDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(CigarCommentModerationStatus.Approved, created.ModerationStatus);

        client.DefaultRequestHeaders.Authorization = null;
        using var get = await client.GetAsync($"/api/reviewcomments?reviewId={reviewId}");
        get.EnsureSuccessStatusCode();
        var list = await get.Content.ReadFromJsonAsync<List<ReviewCommentDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("От модератора к обзору", list[0].Body);
    }

    private static async Task<int> SeedMinimalReviewAsync(AppDbContext db)
    {
        var brand = new Brand
        {
            Name = $"Br_{Guid.NewGuid():N}"[..18],
            CreatedAt = DateTime.UtcNow,
            IsModerated = true,
        };
        db.Brands.Add(brand);
        await db.SaveChangesAsync();

        var cb = new CigarBase
        {
            Name = "Review Comment Cigar",
            BrandId = brand.Id,
            IsModerated = true,
            CreatedAt = DateTime.UtcNow,
        };
        db.CigarBases.Add(cb);
        await db.SaveChangesAsync();

        JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
        var author = new User
        {
            Username = $"ra{Guid.NewGuid():N}"[..10],
            Email = $"{Guid.NewGuid():N}@rev.test",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            Role = Role.User,
        };
        db.Users.Add(author);
        await db.SaveChangesAsync();

        var review = new Review
        {
            Title = "Тестовый обзор",
            Content = "Текст обзора для комментариев.",
            Rating = 8,
            UserId = author.Id,
            CigarBaseId = cb.Id,
            CreatedAt = DateTime.UtcNow,
            SmokingDate = DateTime.UtcNow,
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        return review.Id;
    }
}
