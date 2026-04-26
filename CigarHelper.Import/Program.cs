using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CigarHelper.Api.Options;
using CigarHelper.Api.Storage;
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
            var imageStorage = services.GetRequiredService<IImageStorageProvider>();
            var thumbnails = services.GetRequiredService<IThumbnailGenerator>();
            var imageOpts = services.GetRequiredService<IOptions<ImageStorageOptions>>();

            // Определяем путь к CSV-файлу
            string csvFilePath = GetCsvFilePath(args);

            var importer = new ImportCigarsFromCsv(
                context, imageStorage, thumbnails, imageOpts, logger);
            
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
        var baseParent = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
        if (baseParent != null)
        {
            string parentPath = Path.Combine(baseParent.FullName, "cigarday.csv");
            if (File.Exists(parentPath))
            {
                return parentPath;
            }
        }

        // Если файл не найден, запрашиваем путь у пользователя
        Console.WriteLine("Не удалось найти CSV файл с данными о сигарах.");
        Console.Write("Пожалуйста, укажите полный путь к CSV файлу: ");
        string? path = Console.ReadLine();
        
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            throw new FileNotFoundException("CSV файл не найден");
        }
        
        return path;
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseContentRoot(AppContext.BaseDirectory)
            // User Secrets подключаются только в Development; у консоли по умолчанию Production,
            // если не заданы DOTNET_ENVIRONMENT / ASPNETCORE_ENVIRONMENT — секреты не читались.
            .UseEnvironment(
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environments.Development)
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException(
                            "Задайте ConnectionStrings:DefaultConnection (appsettings, user-secrets или переменная ConnectionStrings__DefaultConnection).")));

                services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
                services.AddSingleton<IImageStorageProvider>(sp =>
                {
                    var opts = sp.GetRequiredService<IOptions<ImageStorageOptions>>().Value;

                    if (opts.Provider.Equals("LocalFile", StringComparison.OrdinalIgnoreCase))
                    {
                        var rootPath = Path.IsPathRooted(opts.LocalPath)
                            ? opts.LocalPath
                            : Path.Combine(AppContext.BaseDirectory, opts.LocalPath);
                        var logger = sp.GetRequiredService<ILogger<LocalFileImageStorage>>();
                        return new LocalFileImageStorage(rootPath, logger);
                    }

                    if (opts.Provider.Equals("Minio", StringComparison.OrdinalIgnoreCase))
                    {
                        var logger = sp.GetRequiredService<ILogger<MinioImageStorageProvider>>();
                        var provider = new MinioImageStorageProvider(opts.Minio, logger);
                        provider.EnsureBucketExistsAsync().GetAwaiter().GetResult();
                        return provider;
                    }

                    throw new InvalidOperationException(
                        $"ImageStorage:Provider '{opts.Provider}' не поддерживается. Укажите Minio или LocalFile.");
                });

                services.AddSingleton<IThumbnailGenerator, ImageSharpThumbnailGenerator>();
                services.AddHttpClient();
            });
}
