using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CigarHelper.Data.Migrations;

public static class DbVersionUpdater
{
    public static void MigrateDatabase(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("Начало процесса миграции базы данных...");
        
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext>();
        
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Миграция базы данных успешно завершена");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при миграции базы данных");
            throw;
        }
    }
} 