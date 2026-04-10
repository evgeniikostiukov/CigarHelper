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

/// <summary>
/// Границы доступа (401/403/404) и контракт ответов, от которых зависит SPA.
/// </summary>
public class ApiAuthorizationAndContractsIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Profile_Get_WithoutToken_Returns401()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.GetAsync("/api/Profile");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Humidors_Get_WithoutToken_Returns401()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.GetAsync("/api/Humidors");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task CigarBasesPaginated_Get_WithoutToken_Returns401()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.GetAsync("/api/cigars/bases/paginated?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task CigarImages_Get_WithoutToken_Returns401()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.GetAsync("/api/CigarImages");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Reviews_Post_WithoutToken_Returns401()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.PostAsJsonAsync("/api/Reviews", new CreateReviewRequest
        {
            Title = "Tst",
            Content = "Body",
            CigarBaseId = 1,
            Rating = 5
        });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task AdminUsers_Get_AsRegularUser_Returns403()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"adm{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        using var res = await client.GetAsync("/api/admin/users?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task AdminCigarImages_Get_WithoutToken_Returns401()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        using var res = await client.GetAsync("/api/admin/cigar-images?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task AdminCigarImages_Get_AsRegularUser_Returns403()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"img{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        using var res = await client.GetAsync("/api/admin/cigar-images?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task AdminCigarImages_Get_AsAdmin_Returns200_PageContract()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        var adminEmail = $"{Guid.NewGuid():N}@admin.ci";
        var adminUsername = $"ad{Guid.NewGuid():N}"[..10];

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
            db.Users.Add(new User
            {
                Username = adminUsername,
                Email = adminEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                Role = Role.Admin
            });
            var brand = new Brand
            {
                Name = $"Bi_{Guid.NewGuid():N}"[..14],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            var cb = new CigarBase
            {
                Name = "Admin Img List CB",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            };
            db.CigarBases.Add(cb);
            await db.SaveChangesAsync();
            db.CigarImages.Add(new CigarImage
            {
                CigarBaseId = cb.Id,
                StoragePath = "adm-list-orig",
                ThumbnailPath = "adm-list-thumb",
                FileName = "a.png",
                ContentType = "image/png",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var loginRes = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Username = adminUsername,
            Password = "abCd12"
        });
        loginRes.EnsureSuccessStatusCode();
        var authBody = await loginRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        using var res = await client.GetAsync("/api/admin/cigar-images?page=1&pageSize=10");
        res.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("items", out var itemsEl));
        Assert.Equal(JsonValueKind.Array, itemsEl.ValueKind);
        Assert.True(root.TryGetProperty("totalCount", out var totalEl) && totalEl.GetInt32() >= 1);
        Assert.True(root.TryGetProperty("page", out var pageEl) && pageEl.GetInt32() == 1);
        Assert.True(root.TryGetProperty("pageSize", out var psEl) && psEl.GetInt32() == 10);
        var first = itemsEl.EnumerateArray().FirstOrDefault();
        Assert.Equal(JsonValueKind.Object, first.ValueKind);
        Assert.True(first.TryGetProperty("id", out _));
        Assert.True(first.TryGetProperty("cigarBaseId", out _));
    }

    [Fact]
    public async Task Humidors_GetById_OtherUser_Returns404()
    {
        await using var factory = new AuthIntegrationWebAppFactory();

        int victimHumidorId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
            var owner = new User
            {
                Username = $"own{Guid.NewGuid():N}"[..10],
                Email = $"{Guid.NewGuid():N}@o.co",
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                Role = Role.User
            };
            db.Users.Add(owner);
            await db.SaveChangesAsync();

            var h = new Humidor
            {
                Name = "Secret",
                Capacity = 50,
                UserId = owner.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Humidors.Add(h);
            await db.SaveChangesAsync();
            victimHumidorId = h.Id;
        }

        using var client = factory.CreateClient();
        var reg = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"intr{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        reg.EnsureSuccessStatusCode();
        var body = await reg.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(body?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);

        using var res = await client.GetAsync($"/api/Humidors/{victimHumidorId}");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task PublicUsers_GetProfile_NewUserPrivate_Returns404()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        const string username = "privpubuser";
        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = username,
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();

        using var res = await client.GetAsync($"/api/public/users/{username}");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task PublicUsers_GetProfile_WhenPublic_ReturnsOk()
    {
        await using var factory = new AuthIntegrationWebAppFactory();

        const string username = "publicprof";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            JwtService.CreatePasswordHash("abCd12", out var hash, out var salt);
            db.Users.Add(new User
            {
                Username = username,
                Email = "publicprof@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                IsProfilePublic = true
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        using var res = await client.GetAsync($"/api/public/users/{username}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task CigarBasesPaginated_Response_HasContract_AndClampsPageSize()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"pg{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        using var res = await client.GetAsync(
            "/api/cigars/bases/paginated?page=1&pageSize=500&excludeBinaryMedia=true");
        res.EnsureSuccessStatusCode();
        var page = await res.Content.ReadFromJsonAsync<PaginatedResult<CigarBaseDto>>(JsonOptions);
        Assert.NotNull(page);
        Assert.NotNull(page!.Items);
        Assert.Equal(20, page.PageSize);
        Assert.Equal(1, page.Page);
        Assert.True(page.TotalCount >= 0);
        Assert.True(page.TotalPages >= 0);
    }

    [Fact]
    public async Task CigarBasesPaginated_SecondPage_EmptyItems_WhenOnlyOneRow()
    {
        await using var factory = new AuthIntegrationWebAppFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var brand = new Brand
            {
                Name = $"Pg3_{Guid.NewGuid():N}"[..18],
                CreatedAt = DateTime.UtcNow,
                IsModerated = true
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            db.CigarBases.Add(new CigarBase
            {
                Name = "Single Page Test",
                BrandId = brand.Id,
                IsModerated = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"p2{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        using var res = await client.GetAsync(
            "/api/cigars/bases/paginated?page=2&pageSize=20&excludeBinaryMedia=true");
        res.EnsureSuccessStatusCode();
        var page = await res.Content.ReadFromJsonAsync<PaginatedResult<CigarBaseDto>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(2, page!.Page);
        Assert.True(page.TotalCount >= 1);
        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task CatalogMutation_AsRegularUser_BrandsPost_Returns403()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"u{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        using var res = await client.PostAsJsonAsync("/api/Brands", new CreateBrandRequest
        {
            Name = $"Brand_{Guid.NewGuid():N}"[..20],
            IsModerated = false
        });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task CatalogMutation_AsRegularUser_CigarBasesPost_Returns403()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        using var client = factory.CreateClient();

        var registerRes = await client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
        {
            Username = $"u{Guid.NewGuid():N}"[..10],
            Password = "abCd12",
            ConfirmPassword = "abCd12"
        });
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        using var content = new MultipartFormDataContent();
        using var res = await client.PostAsync("/api/cigars/bases", content);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task CatalogMutation_AsModerator_PostBrand_Returns201()
    {
        await using var factory = new AuthIntegrationWebAppFactory();
        var modUsername = $"mod{Guid.NewGuid():N}"[..10];
        var modEmail = $"{Guid.NewGuid():N}@mod.ci";

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
                Role = Role.Moderator
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var loginRes = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            Username = modUsername,
            Password = "abCd12"
        });
        loginRes.EnsureSuccessStatusCode();
        var authBody = await loginRes.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authBody?.Token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authBody.Token);

        var brandName = $"ModBrand_{Guid.NewGuid():N}"[..24];
        using var res = await client.PostAsJsonAsync("/api/Brands", new CreateBrandRequest
        {
            Name = brandName,
            IsModerated = true
        });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }
}
