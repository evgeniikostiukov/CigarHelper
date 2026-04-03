using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
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
}
