using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using CigarHelper.Data.Data;
using CigarHelper.Import;

namespace CigarHelper.Import;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var host = CreateHostBuilder(args).Build();
        
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var logger = services.GetRequiredService<ILogger<ImportCigarsFromCsv>>();
            
            // Определяем путь к CSV-файлу
            string csvFilePath = GetCsvFilePath(args);
            
            // Создаем экземпляр ImportCigarsFromCsv
            var importer = new ImportCigarsFromCsv(context, logger);
            
            // Запускаем импорт
            await importer.ImportAsync(csvFilePath);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Произошла ошибка во время выполнения");
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static string GetCsvFilePath(string[] args)
    {
        // Сначала проверяем переданные аргументы
        if (args.Length > 0 && File.Exists(args[0]))
        {
            return args[0];
        }

        // Затем проверяем текущую директорию
        string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cigarday.csv");
        if (File.Exists(defaultPath))
        {
            return defaultPath;
        }
        
        // Проверяем родительскую директорию
        string parentPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName, "cigarday.csv");
        if (File.Exists(parentPath))
        {
            return parentPath;
        }
        
        // Если файл не найден, запрашиваем путь у пользователя
        Console.WriteLine("Не удалось найти CSV файл с данными о сигарах.");
        Console.Write("Пожалуйста, укажите полный путь к CSV файлу: ");
        string path = Console.ReadLine();
        
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            throw new FileNotFoundException("CSV файл не найден");
        }
        
        return path;
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .Build();
                
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
                
                services.AddHttpClient();
            });
}
