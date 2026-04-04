using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CigarHelper.Api.Tests;

public class CigarsBasesPaginatedIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task GetCigarBasesPaginated_ModeratedWithImage_ReturnsOk()
    {
        await using var factory = new AuthIntegrationWebAppFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var brand = new Brand
            {
                Name = $"Brand_{Guid.NewGuid():N}"[..20],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            var cb = new CigarBase
            {
                Name = "Paginated Test Cigar",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            };
            db.CigarBases.Add(cb);
            await db.SaveChangesAsync();

            db.CigarImages.Add(new CigarImage
            {
                CigarBaseId = cb.Id,
                StoragePath = "test-object-main-1",
                ThumbnailPath = "test-object-thumb-1",
                FileName = "x.png",
                ContentType = "image/png",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();

        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"u{Guid.NewGuid():N}"[..12],
            Email = $"{Guid.NewGuid():N}@t.co",
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var authBody = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        using var res = await client.GetAsync("/api/cigars/bases/paginated?page=1&pageSize=100");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task GetCigarBasesPaginated_ExcludeBinaryMedia_ReturnsOk()
    {
        await using var factory = new AuthIntegrationWebAppFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var brand = new Brand
            {
                Name = $"B2_{Guid.NewGuid():N}"[..18],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true,
                LogoBytes = new byte[] { 9, 9, 9 }
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            var cb = new CigarBase
            {
                Name = "Lightweight Row",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            };
            db.CigarBases.Add(cb);
            await db.SaveChangesAsync();

            db.CigarImages.Add(new CigarImage
            {
                CigarBaseId = cb.Id,
                StoragePath = "test-object-main-2",
                FileName = "p.png",
                ContentType = "image/png",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();

        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"v{Guid.NewGuid():N}"[..11],
            Email = $"{Guid.NewGuid():N}@x.co",
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var authBody = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        using var res = await client.GetAsync(
            "/api/cigars/bases/paginated?page=1&pageSize=100&excludeBinaryMedia=true");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task GetCigarBasesPaginated_UnmoderatedOnly_AsUser_Ignored_StillModeratedOnly()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        const string moderatedName = "Mod_List_User_Test";
        const string unmoderatedName = "Umod_List_User_Test";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var brand = new Brand
            {
                Name = $"BrU_{Guid.NewGuid():N}"[..16],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            db.CigarBases.Add(new CigarBase
            {
                Name = moderatedName,
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            });
            db.CigarBases.Add(new CigarBase
            {
                Name = unmoderatedName,
                BrandId = brand.Id,
                IsModerated = false,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"um{Guid.NewGuid():N}"[..10],
            Email = $"{Guid.NewGuid():N}@u.co",
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var authBody = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        using var res = await client.GetAsync(
            "/api/cigars/bases/paginated?page=1&pageSize=100&excludeBinaryMedia=true&unmoderatedOnly=true");
        res.EnsureSuccessStatusCode();
        var page = await res.Content.ReadFromJsonAsync<PaginatedResult<CigarBaseDto>>(JsonOptions);
        Assert.NotNull(page?.Items);
        Assert.Contains(page!.Items, x => x.Name == moderatedName);
        Assert.DoesNotContain(page.Items, x => x.Name == unmoderatedName);
    }

    [Fact]
    public async Task GetCigarBasesPaginated_UnmoderatedOnly_AsModerator_ReturnsOnlyUnmoderated()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        const string moderatedName = "Mod_List_Mod_Test";
        const string unmoderatedName = "Umod_List_Mod_Test";
        var modEmail = $"{Guid.NewGuid():N}@mod.test";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
            db.Users.Add(new User
            {
                Username = $"mod{Guid.NewGuid():N}"[..10],
                Email = modEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                Role = Role.Moderator
            });

            var brand = new Brand
            {
                Name = $"BrM_{Guid.NewGuid():N}"[..16],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            db.CigarBases.Add(new CigarBase
            {
                Name = moderatedName,
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            });
            db.CigarBases.Add(new CigarBase
            {
                Name = unmoderatedName,
                BrandId = brand.Id,
                IsModerated = false,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var loginRes = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Email = modEmail,
            Password = "abCd12"
        });
        loginRes.EnsureSuccessStatusCode();
        var authBody = await loginRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        using var res = await client.GetAsync(
            "/api/cigars/bases/paginated?page=1&pageSize=100&excludeBinaryMedia=true&unmoderatedOnly=true");
        res.EnsureSuccessStatusCode();
        var page = await res.Content.ReadFromJsonAsync<PaginatedResult<CigarBaseDto>>(JsonOptions);
        Assert.NotNull(page?.Items);
        Assert.DoesNotContain(page!.Items, x => x.Name == moderatedName);
        var hit = Assert.Single(page.Items.Where(x => x.Name == unmoderatedName));
        Assert.False(hit.IsModerated);
    }
}
