using CigarHelper.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CigarHelper.Api.Tests;

/// <summary>Хост API в окружении Testing: InMemory EF задаётся в Program, без второго провайдера.</summary>
public class AuthIntegrationWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=inmemory;");
        builder.UseSetting("Jwt:Key", new string('k', 32));
        builder.UseSetting("Jwt:Issuer", "integration-test");
        builder.UseSetting("Jwt:Audience", "integration-test");
    }
}
