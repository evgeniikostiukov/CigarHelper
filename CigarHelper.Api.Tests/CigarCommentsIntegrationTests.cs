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

public class CigarCommentsIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions =  new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task GetCigarComments_WithoutTarget_Returns400()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.GetAsync("/api/cigarcomments");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task GetCigarComments_Anonymous_ModeratedBase_ReturnsOk()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        int cbId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var brand = new Brand
            {
                Name = $"Bc_{Guid.NewGuid():N}"[..18],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            var cb = new CigarBase
            {
                Name = "Comment Base",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            };
            db.CigarBases.Add(cb);
            await db.SaveChangesAsync();
            cbId = cb.Id;
        }

        using var client = factory.CreateClient();
        using var res = await client.GetAsync($"/api/cigarcomments?cigarBaseId={cbId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var list = await res.Content.ReadFromJsonAsync<List<CigarCommentDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task PostCigarComment_ByUser_Pending_ThenModeratorApproves_AnonymousSeesComment()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        int cbId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var brand = new Brand
            {
                Name = $"Bb_{Guid.NewGuid():N}"[..18],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            var cb = new CigarBase
            {
                Name = "Comment Base Two",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            };
            db.CigarBases.Add(cb);
            await db.SaveChangesAsync();
            cbId = cb.Id;
        }

        using var client = factory.CreateClient();
        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"cu{Guid.NewGuid():N}"[..11],
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = true
        });
        registerRes.EnsureSuccessStatusCode();
        var authBody = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        using var post = await client.PostAsJsonAsync("/api/cigarcomments", new CreateCigarCommentRequest
        {
            CigarBaseId = cbId,
            Body = "  Привет из теста  "
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var created = await post.Content.ReadFromJsonAsync<CigarCommentDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(CigarCommentModerationStatus.Pending, created.ModerationStatus);

        client.DefaultRequestHeaders.Authorization = null;
        using (var getEmpty = await client.GetAsync($"/api/cigarcomments?cigarBaseId={cbId}"))
        {
            getEmpty.EnsureSuccessStatusCode();
            var emptyList = await getEmpty.Content.ReadFromJsonAsync<List<CigarCommentDto>>(JsonOptions);
            Assert.NotNull(emptyList);
            Assert.Empty(emptyList);
        }

        var modUsername = $"mc{Guid.NewGuid():N}"[..9];
        var modEmail = $"{Guid.NewGuid():N}@modcmt.test";
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
            $"/api/admin/cigar-comments/{created.Id}/approve",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        using var get = await client.GetAsync($"/api/cigarcomments?cigarBaseId={cbId}");
        get.EnsureSuccessStatusCode();
        var list = await get.Content.ReadFromJsonAsync<List<CigarCommentDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("Привет из теста", list[0].Body);
        Assert.Equal(CigarCommentModerationStatus.Approved, list[0].ModerationStatus);
    }

    [Fact]
    public async Task PostCigarComment_ByModerator_IsApprovedImmediately_OnPublicList()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        int cbId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var brand = new Brand
            {
                Name = $"Bm_{Guid.NewGuid():N}"[..18],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            var cb = new CigarBase
            {
                Name = "Mod Writes Comment",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            };
            db.CigarBases.Add(cb);
            await db.SaveChangesAsync();
            cbId = cb.Id;
        }

        var modUsername = $"mw{Guid.NewGuid():N}"[..9];
        var modEmail = $"{Guid.NewGuid():N}@modw.test";
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

        using var post = await client.PostAsJsonAsync("/api/cigarcomments", new CreateCigarCommentRequest
        {
            CigarBaseId = cbId,
            Body = "От модератора",
        });
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<CigarCommentDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(CigarCommentModerationStatus.Approved, created.ModerationStatus);

        client.DefaultRequestHeaders.Authorization = null;
        using var get = await client.GetAsync($"/api/cigarcomments?cigarBaseId={cbId}");
        get.EnsureSuccessStatusCode();
        var list = await get.Content.ReadFromJsonAsync<List<CigarCommentDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("От модератора", list[0].Body);
    }

    [Fact]
    public async Task PostUserCigarComment_OwnerCannotComment_VisitorOk()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        var ownerUsername = $"ow{Guid.NewGuid():N}"[..10];
        var ownerReg = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = ownerUsername,
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = true
        });
        ownerReg.EnsureSuccessStatusCode();
        var ownerAuth = await ownerReg.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(ownerAuth?.Token);

        int ucId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var owner = await db.Users.FirstAsync(u => u.Username == ownerUsername);
            owner.IsProfilePublic = true;

            var brand = new Brand
            {
                Name = $"Bo_{Guid.NewGuid():N}"[..18],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            var cb = new CigarBase
            {
                Name = "UC Comment Base",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            };
            db.CigarBases.Add(cb);
            await db.SaveChangesAsync();

            var uc = new UserCigar
            {
                CigarBaseId = cb.Id,
                UserId = owner.Id,
                CreatedAt = DateTime.UtcNow,
                LastTouchedAt = DateTime.UtcNow,
            };
            db.UserCigars.Add(uc);
            await db.SaveChangesAsync();
            ucId = uc.Id;
        }

        var visitorReg = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"vi{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12",
            ConfirmedAge18 = true
        });
        visitorReg.EnsureSuccessStatusCode();
        var visitorAuth = await visitorReg.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(visitorAuth?.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", visitorAuth.Token);
        using var visitorPost = await client.PostAsJsonAsync("/api/cigarcomments", new CreateCigarCommentRequest
        {
            UserCigarId = ucId,
            Body = "От гостя"
        });
        Assert.Equal(HttpStatusCode.Created, visitorPost.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerAuth.Token);
        using var ownerPost = await client.PostAsJsonAsync("/api/cigarcomments", new CreateCigarCommentRequest
        {
            UserCigarId = ucId,
            Body = "От владельца"
        });
        Assert.Equal(HttpStatusCode.BadRequest, ownerPost.StatusCode);
    }
}
