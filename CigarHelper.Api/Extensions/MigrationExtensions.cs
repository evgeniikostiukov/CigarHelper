using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CigarHelper.Api.Extensions;

public static class MigrationExtensions
{
    /// <summary>
    /// Applies any pending migrations to the database
    /// </summary>
    /// <param name="app">The web application to apply migrations for</param>
    /// <param name="logger">Optional logger for migration information</param>
    /// <typeparam name="T">The DbContext type</typeparam>
    public static IHost ApplyMigrations<T>(this IHost app, ILogger? logger = null) where T : DbContext
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<T>();
            logger?.LogInformation("Applying migrations for {DbContext}", typeof(T).Name);
            context.Database.Migrate();
            logger?.LogInformation("Migrations applied successfully for {DbContext}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while applying migrations for {DbContext}", typeof(T).Name);
            throw;
        }
        
        return app;
    }
} 