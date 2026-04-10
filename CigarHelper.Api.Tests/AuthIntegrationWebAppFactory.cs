using CigarHelper.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CigarHelper.Api.Tests;

/// <summary>Хост API в окружении Testing: InMemory EF задаётся в Program, без второго провайдера.</summary>
public class AuthIntegrationWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _imageStoragePath =
        Path.Combine(Path.GetTempPath(), "CigarHelperApiTests", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=inmemory;");
        builder.UseSetting("Jwt:Key", new string('k', 32));
        builder.UseSetting("Jwt:Issuer", "integration-test");
        builder.UseSetting("Jwt:Audience", "integration-test");
        // Без MinIO в CI/тестах: файлы в temp (та же схема ключей, что и LocalFile в API).
        builder.UseSetting("ImageStorage:Provider", "LocalFile");
        builder.UseSetting("ImageStorage:LocalPath", _imageStoragePath);
    }
}
