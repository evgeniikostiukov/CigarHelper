using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using CigarHelper.Api.Options;
using CigarHelper.Api.Storage;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CigarHelper.Import;

public class ImportCigarsFromCsv
{
    private readonly AppDbContext _context;
    private readonly Dictionary<string, Brand> _brandCache = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImportCigarsFromCsv> _logger;
    private readonly IImageStorageProvider _imageStorage;
    private readonly IThumbnailGenerator _thumbnails;
    private readonly ImageStorageOptions _imageStorageOptions;

    // Словарь с информацией о брендах (страна и описание)
    private readonly Dictionary<string, (string Country, string Description)> _brandInfo = new()
    {
        // Кубинские бренды
        ["Cohiba"] = ("Куба", "Премиальный кубинский бренд сигар, созданный в 1966 году. Известен своим исключительным качеством и сложными вкусами."),
        ["Montecristo"] = ("Куба", "Один из самых известных кубинских брендов сигар, основанный в 1935 году. Назван в честь романа Александра Дюма."),
        ["Romeo Y Julieta"] = ("Куба", "Классический кубинский бренд, основанный в 1875 году. Известен своими мягкими и сбалансированными сигарами."),
        ["Partagas"] = ("Куба", "Кубинский бренд, основанный в 1845 году. Известен своими крепкими и ароматными сигарами."),
        ["Trinidad"] = ("Куба", "Элитный кубинский бренд, созданный в 1969 году. Производит ограниченные серии высококачественных сигар."),
        ["Hoyo De Monterrey"] = ("Куба", "Кубинский бренд, основанный в 1865 году. Известен своими мягкими и элегантными сигарами."),
        ["H.Upmann"] = ("Куба", "Кубинский бренд, основанный в 1844 году. Известен своими сложными и изысканными вкусами."),
        ["Bolivar"] = ("Куба", "Кубинский бренд, основанный в 1902 году. Известен своими крепкими и ароматными сигарами."),
        ["Diplomaticos"] = ("Куба", "Кубинский бренд, созданный в 1966 году. Производит сигары среднего размера с классическими кубинскими вкусами."),
        ["Fonseca"] = ("Куба", "Кубинский бренд, основанный в 1906 году. Известен своими мягкими и ароматными сигарами."),
        ["Quintero"] = ("Куба", "Кубинский бренд, основанный в 1924 году. Производит доступные сигары с классическими кубинскими вкусами."),
        ["Quai d'Orsay"] = ("Куба", "Кубинский бренд, созданный в 1973 году специально для французского рынка. Известен своими элегантными сигарами."),
        ["Ramon Allones"] = ("Куба", "Кубинский бренд, основанный в 1837 году. Один из старейших брендов с богатой историей."),
        ["Rey Del Mundo"] = ("Куба", "Кубинский бренд, основанный в 1848 году. Известен своими традиционными методами производства."),
        ["Sancho Panza"] = ("Куба", "Кубинский бренд, основанный в 1848 году. Назван в честь персонажа из романа Сервантеса."),
        ["Vegas Robaina"] = ("Куба", "Кубинский бренд, созданный в 1997 году. Назван в честь известного производителя табака."),
        ["Vegueros"] = ("Куба", "Кубинский бренд, созданный в 1997 году. Производит доступные сигары для местного рынка."),
        ["Juan Lopez"] = ("Куба", "Кубинский бренд, основанный в 1876 году. Известен своими традиционными кубинскими вкусами."),
        ["Por Larranaga"] = ("Куба", "Кубинский бренд, основанный в 1834 году. Один из старейших брендов с богатой историей."),
        ["La Gloria Cubana"] = ("Куба", "Кубинский бренд, основанный в 1885 году. Известен своими классическими кубинскими сигарами."),
        ["La Flor De Cano"] = ("Куба", "Кубинский бренд, основанный в 1884 году. Производит традиционные кубинские сигары."),
        ["La Unica"] = ("Куба", "Кубинский бренд, известный своими доступными сигарами с классическими вкусами."),
        ["La Instructora"] = ("Куба", "Кубинский бренд, производящий сигары для местного рынка."),
        ["La Ley"] = ("Куба", "Кубинский бренд с традиционными методами производства."),
        ["La Estrella"] = ("Куба", "Кубинский бренд, производящий классические сигары."),
        ["La Galera"] = ("Куба", "Кубинский бренд с богатой историей производства сигар."),
        ["S.Cristobal"] = ("Куба", "Кубинский бренд, созданный в 1999 году. Назван в честь покровителя Гаваны."),
        ["Siglo de Oro"] = ("Куба", "Кубинский бренд, производящий премиальные сигары с золотым веком кубинских сигар."),
        ["Saint Luis Rey"] = ("Куба", "Кубинский бренд, основанный в 1940-х годах. Назван в честь романа Сент-Экзюпери."),
        ["Cuaba"] = ("Куба", "Кубинский бренд, созданный в 1996 году. Известен своими фигурированными сигарами."),
        ["Guantanamera"] = ("Куба", "Кубинский бренд, названный в честь популярной кубинской песни. Производит доступные сигары."),
        ["ORISHAS"] = ("Куба", "Кубинский бренд, вдохновленный афро-кубинской религией. Производит сигары с уникальными вкусами."),
        ["Pelo de Oro"] = ("Куба", "Кубинский бренд, производящий сигары с золотистым табаком."),
        ["Jose L.Piedra"] = ("Куба", "Кубинский бренд, производящий доступные сигары для местного рынка."),
        
        // Доминиканские бренды
        ["La Aurora"] = ("Доминиканская Республика", "Доминиканский бренд, основанный в 1903 году. Старейший производитель сигар в Доминиканской Республике."),
        ["La Aroma Del Caribe"] = ("Доминиканская Республика", "Доминиканский бренд, известный своими ароматными сигарами с карибскими вкусами."),
        ["La Flor Dominicana"] = ("Доминиканская Республика", "Доминиканский бренд, основанный в 1994 году. Известен своими крепкими и ароматными сигарами."),
        ["Samana"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с табаком из региона Самана."),
        ["Santa Damiana"] = ("Доминиканская Республика", "Доминиканский бренд, известный своими традиционными методами производства."),
        ["Toreo"] = ("Доминиканская Республика", "Доминиканский бренд, вдохновленный испанской корридой."),
        ["Torres"] = ("Доминиканская Республика", "Доминиканский бренд с традиционными методами производства."),
        ["Total Flame"] = ("Доминиканская Республика", "Доминиканский бренд с яркими и интенсивными вкусами."),
        ["Vegafina"] = ("Доминиканская Республика", "Доминиканский бренд, производящий доступные сигары с классическими вкусами."),
        ["Vintage"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с выдержанным табаком."),
        ["XO"] = ("Доминиканская Республика", "Доминиканский бренд, производящий премиальные сигары с эксклюзивными вкусами."),
        ["Yoruba"] = ("Доминиканская Республика", "Доминиканский бренд, вдохновленный африканской культурой."),
        ["Zapata"] = ("Доминиканская Республика", "Доминиканский бренд, названный в честь мексиканского революционера."),
        ["Zino"] = ("Доминиканская Республика", "Доминиканский бренд от Davidoff, производящий элегантные сигары."),
        ["Zino Platinum"] = ("Доминиканская Республика", "Премиальная линия бренда Zino от Davidoff с эксклюзивными сигарами."),
        ["Alfambra"] = ("Доминиканская Республика", "Доминиканский бренд с традиционными методами производства."),
        ["Aristocrat"] = ("Доминиканская Республика", "Доминиканский бренд, производящий элегантные сигары."),
        ["Ashton"] = ("Доминиканская Республика", "Американский бренд, производящий сигары в Доминиканской Республике. Известен своим качеством."),
        ["Atabey"] = ("Доминиканская Республика", "Доминиканский бренд, производящий премиальные сигары с уникальными вкусами."),
        ["AVO"] = ("Доминиканская Республика", "Доминиканский бренд от Davidoff, основанный Avo Uvezian. Известен своими элегантными сигарами."),
        ["Bentley"] = ("Доминиканская Республика", "Доминиканский бренд, вдохновленный роскошными автомобилями."),
        ["Buena Vista"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с карибскими вкусами."),
        ["Caldwell"] = ("Доминиканская Республика", "Доминиканский бренд, основанный Robert Caldwell. Известен своими инновационными смесями."),
        ["Capadura"] = ("Доминиканская Республика", "Доминиканский бренд с традиционными методами производства."),
        ["Carlos Andre"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с классическими вкусами."),
        ["Cherokee"] = ("Доминиканская Республика", "Доминиканский бренд, вдохновленный индейской культурой."),
        ["Cumpay"] = ("Доминиканская Республика", "Доминиканский бренд с традиционными методами производства."),
        ["Cusano"] = ("Доминиканская Республика", "Доминиканский бренд, основанный Michael Chiusano. Известен своими качественными сигарами."),
        ["Davidoff"] = ("Доминиканская Республика", "Швейцарский бренд, производящий сигары в Доминиканской Республике. Известен своим премиальным качеством."),
        ["Don Tomas"] = ("Доминиканская Республика", "Доминиканский бренд, основанный в 1975 году. Производит доступные сигары с классическими вкусами."),
        ["Don Diego"] = ("Доминиканская Республика", "Доминиканский бренд, производящий элегантные сигары с классическими вкусами."),
        ["Ernesto Perez"] = ("Доминиканская Республика", "Доминиканский бренд, названный в честь известного производителя сигар."),
        ["Furia"] = ("Доминиканская Республика", "Доминиканский бренд, производящий крепкие сигары с интенсивными вкусами."),
        ["God Of Fire"] = ("Доминиканская Республика", "Доминиканский бренд, производящий премиальные сигары с эксклюзивными вкусами."),
        ["Griffins"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с классическими вкусами."),
        ["Gurkha"] = ("Доминиканская Республика", "Американский бренд, производящий сигары в Доминиканской Республике. Известен своими премиальными сигарами."),
        ["Horacio"] = ("Доминиканская Республика", "Доминиканский бренд, названный в честь известного производителя сигар."),
        ["Leon Jimenes"] = ("Доминиканская Республика", "Доминиканский бренд, основанный в 1903 году. Один из старейших производителей в Доминиканской Республике."),
        ["Luis Martinez"] = ("Доминиканская Республика", "Доминиканский бренд, названный в честь известного производителя сигар."),
        ["Macanudo"] = ("Доминиканская Республика", "Американский бренд, производящий сигары в Доминиканской Республике. Известен своими мягкими сигарами."),
        ["Miro"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с классическими вкусами."),
        ["Montosa"] = ("Доминиканская Республика", "Доминиканский бренд с традиционными методами производства."),
        ["Parcero"] = ("Доминиканская Республика", "Доминиканский бренд с традиционными методами производства."),
        ["PDR"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с классическими вкусами."),
        
        // Никарагуанские бренды
        ["AJ Fernandez"] = ("Никарагуа", "Никарагуанский бренд, основанный A.J. Fernandez. Известен своими крепкими и ароматными сигарами."),
        ["Asylum"] = ("Никарагуа", "Никарагуанский бренд, производящий крепкие и интенсивные сигары."),
        ["Cain"] = ("Никарагуа", "Никарагуанский бренд, производящий крепкие сигары с интенсивными вкусами."),
        ["CAO"] = ("Никарагуа", "Американский бренд, производящий сигары в Никарагуа. Известен своими инновационными смесями."),
        ["CLE"] = ("Никарагуа", "Никарагуанский бренд, основанный Christian Eiroa. Известен своими качественными сигарами."),
        ["Cuba Aliados"] = ("Никарагуа", "Никарагуанский бренд, основанный в 1927 году. Производит сигары с кубинскими традициями."),
        ["Diesel"] = ("Никарагуа", "Никарагуанский бренд, производящий крепкие сигары с интенсивными вкусами."),
        ["Humo Jaguar"] = ("Никарагуа", "Никарагуанский бренд, вдохновленный майянской культурой."),
        ["My Father"] = ("Никарагуа", "Никарагуанский бренд, основанный Jose Pepin Garcia. Известен своими крепкими и ароматными сигарами."),
        ["Nicarao"] = ("Никарагуа", "Никарагуанский бренд, названный в честь древнего народа Никарао."),
        ["NUB"] = ("Никарагуа", "Никарагуанский бренд, производящий короткие сигары с интенсивными вкусами."),
        ["Oliva"] = ("Никарагуа", "Никарагуанский бренд, основанный в 1886 году. Известен своими качественными сигарами."),
        ["Padron"] = ("Никарагуа", "Никарагуанский бренд, основанный Jose Padron. Известен своими премиальными сигарами."),
        ["Perdomo"] = ("Никарагуа", "Никарагуанский бренд, основанный Nick Perdomo. Известен своими качественными сигарами."),
        ["Perla Del Mar"] = ("Никарагуа", "Никарагуанский бренд, производящий сигары с морскими вкусами."),
        ["Plasensia"] = ("Никарагуа", "Никарагуанский бренд, основанный Nestor Plasencia. Известен своими качественными сигарами."),
        ["San Lotano"] = ("Никарагуа", "Никарагуанский бренд, основанный A.J. Fernandez. Известен своими крепкими сигарами."),
        ["Sicario"] = ("Никарагуа", "Никарагуанский бренд, производящий крепкие и ароматные сигары."),
        
        // Гондурасские бренды
        ["Alec Bradley"] = ("Гондурас", "Американский бренд, производящий сигары в Гондурасе. Известен своими инновационными смесями."),
        ["Camacho"] = ("Гондурас", "Гондурасский бренд, основанный в 1961 году. Известен своими крепкими сигарами."),
        ["Eiroa"] = ("Гондурас", "Гондурасский бренд, основанный Julio Eiroa. Известен своими традиционными методами производства."),
        ["Flor De Copan"] = ("Гондурас", "Гондурасский бренд, производящий сигары с табаком из региона Копан."),
        ["Rocky Patel"] = ("Гондурас", "Американский бренд, производящий сигары в Гондурасе. Известен своими качественными сигарами."),
        ["Villa Vieja"] = ("Гондурас", "Гондурасский бренд, производящий традиционные сигары."),
        ["Villa Zamorano"] = ("Гондурас", "Гондурасский бренд от Maya Selva, известный своими качественными сигарами."),
        
        // Мексиканские бренды
        ["Casa 1910"] = ("Мексика", "Мексиканский бренд, основанный в 1910 году. Производит сигары с мексиканскими традициями."),
        ["Casa Turrent"] = ("Мексика", "Мексиканский бренд, основанный в 1880 году. Известен своими традиционными методами производства."),
        ["Te Amo"] = ("Мексика", "Мексиканский бренд, основанный в 1960-х годах. Производит сигары с мексиканскими традициями."),
        
        // Европейские бренды
        ["Balmoral"] = ("Нидерланды", "Голландский бренд, производящий сигары с европейскими традициями."),
        ["Bossner"] = ("Германия", "Немецкий бренд, производящий сигары с европейскими традициями."),
        ["Stanislaw"] = ("Польша", "Польский бренд, производящий сигары с европейскими традициями."),
        ["Toscano"] = ("Италия", "Итальянский бренд, основанный в 1818 году. Производит традиционные итальянские сигары."),
        
        // Азиатские бренды
        ["Ararat"] = ("Армения", "Армянский бренд, производящий сигары с местными традициями."),
        ["Davtian"] = ("Армения", "Армянский бренд, производящий сигары с местными традициями."),
        ["Great Wall"] = ("Китай", "Китайский бренд, производящий сигары с восточными традициями."),
        
        // Российские бренды
        ["Pogarskaya Fabrika"] = ("Россия", "Российский бренд, производящий сигары с местными традициями."),
        ["Евгений Онегин"] = ("Россия", "Российский бренд, названный в честь романа Пушкина. Производит сигары с русскими традициями."),
        
        // Турецкие бренды
        ["Dardanelles"] = ("Турция", "Турецкий бренд, производящий сигары с восточными традициями."),
        
        // Другие бренды
        ["Brick House"] = ("Никарагуа", "Никарагуанский бренд, производящий доступные сигары с классическими вкусами."),
        ["Casa 1910"] = ("Мексика", "Мексиканский бренд, основанный в 1910 году. Производит сигары с мексиканскими традициями."),
        ["Dominican Estates"] = ("Доминиканская Республика", "Доминиканский бренд, производящий сигары с классическими вкусами."),
        ["Drew Estate"] = ("Никарагуа", "Американский бренд, производящий сигары в Никарагуа. Известен своими инновационными смесями."),
        ["El Baton"] = ("Никарагуа", "Никарагуанский бренд, производящий сигары с классическими вкусами."),
        ["Flor De Selva"] = ("Гондурас", "Гондурасский бренд, производящий сигары с табаком из тропических лесов."),
        ["Joya De Nicaragua"] = ("Никарагуа", "Никарагуанский бренд, основанный в 1968 году. Известен своими качественными сигарами."),
        ["Other"] = ("Неизвестно", "Бренд с неизвестным происхождением.")
    };

    public ImportCigarsFromCsv(
        AppDbContext context,
        IImageStorageProvider imageStorage,
        IThumbnailGenerator thumbnails,
        IOptions<ImageStorageOptions> imageStorageOptions,
        ILogger<ImportCigarsFromCsv>? logger = null)
    {
        _context = context;
        _imageStorage = imageStorage;
        _thumbnails = thumbnails;
        _imageStorageOptions = imageStorageOptions.Value;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger = logger ?? NullLogger<ImportCigarsFromCsv>.Instance;
    }

    public async Task ImportAsync(string csvFilePath)
    {
        Console.WriteLine("Начинаем импорт сигар из CSV файла...");
        
        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine($"Ошибка: Файл {csvFilePath} не найден");
            return;
        }

        var lines = await File.ReadAllLinesAsync(csvFilePath);
        if (lines.Length <= 1)
        {
            Console.WriteLine("CSV файл пуст или содержит только заголовки");
            return;
        }

        var cigars = ParseCsvLines(lines.Skip(1).ToArray());
        Console.WriteLine($"Найдено {cigars.Count} сигар для импорта");

        await ImportBrandsAsync(cigars);
        await ImportCigarsAsync(cigars);
        
        Console.WriteLine("Импорт завершен успешно!");
    }

    private List<CigarData> ParseCsvLines(string[] lines)
    {
        var cigars = new List<CigarData>();
        
        foreach (var line in lines)
        {
            try
            {
                var fields = ParseCsvLine(line);
                if (fields.Length >= 5)
                {
                    var cigar = new CigarData
                    {
                        Url = fields[0].Trim('"'),
                        ImageUrl = fields[1].Trim('"'),
                        Size = fields[2].Trim('"'),
                        Name = fields[3].Trim('"'),
                        Price = fields[4].Trim('"')
                    };
                    
                    cigars.Add(cigar);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка парсинга строки: {ex.Message}");
            }
        }
        
        return cigars;
    }

    private string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = "";
        var inQuotes = false;
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        fields.Add(currentField);
        return fields.ToArray();
    }

    private async Task ImportBrandsAsync(List<CigarData> cigars)
    {
        Console.WriteLine("Импортируем бренды...");
        
        var brandNames = cigars
            .Select(c => ExtractBrandName(c.Name))
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .ToList();


        foreach (var brandName in brandNames)
        {
            var existingBrand = await _context.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == brandName.ToLower());
                
            if (existingBrand == null)
            {
                // Используем именованный кортеж, чтобы сохранить доступ к полям Country и Description
                var brandInfo = _brandInfo.ContainsKey(brandName)
                    ? _brandInfo[brandName]
                    : (Country: "Неизвестно", Description: $"Бренд {brandName} с неизвестным происхождением.");
                
                var newBrand = new Brand
                {
                    Name = brandName,
                    Country = brandInfo.Country,
                    Description = brandInfo.Description,
                    IsModerated = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Brands.Add(newBrand);
                await _context.SaveChangesAsync();
                
                _brandCache[brandName.ToLower()] = newBrand;
                Console.WriteLine($"Создан бренд: {brandName} ({brandInfo.Country})");
            }
            else
            {
                _brandCache[brandName.ToLower()] = existingBrand;
            }
        }
    }

    private async Task ImportCigarsAsync(List<CigarData> cigars)
    {
        Console.WriteLine("Импортируем сигары...");
        
        var importedCount = 0;
        var skippedCount = 0;
        var imagesCount = 0;
        var imagesErrorCount = 0;
        
        var addedCigars = new List<string>();
        var progressTick = 0;

        foreach (var cigarData in cigars)
        {
            try
            {
                progressTick++;
                var brandName = ExtractBrandName(cigarData.Name);
                if (string.IsNullOrEmpty(brandName) || !_brandCache.ContainsKey(brandName.ToLower()))
                {
                    skippedCount++;
                    continue;
                }

                var brand = _brandCache[brandName.ToLower()];

                var importImageFileName = string.IsNullOrEmpty(cigarData.ImageUrl)
                    ? string.Empty
                    : GetFileNameFromUrl(cigarData.ImageUrl);

                // Проверяем, не существует ли уже такая сигара
                var existingCigar = await _context.CigarBases
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == cigarData.Name.ToLower() && c.BrandId == brand.Id);

                CigarBase cigar;
                if (existingCigar != null)
                {
                    var hasMainImage = await _context.CigarImages.AnyAsync(
                        ci => ci.CigarBaseId == existingCigar.Id && ci.IsMain,
                        CancellationToken.None);
                    if (hasMainImage)
                    {
                        skippedCount++;
                        continue;
                    }

                    cigar = existingCigar;
                }
                else
                {
                    cigar = new CigarBase
                    {
                        Name = cigarData.Name,
                        BrandId = brand.Id,
                        Country = brand.Country ?? "Неизвестно", // Используем страну бренда
                        IsModerated = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    ApplyVitolaFromCsvField(cigar, cigarData.Size);

                    if (addedCigars.Contains(cigar.Name))
                    {
                        skippedCount++;
                        continue;
                    }

                    _context.CigarBases.Add(cigar);
                    addedCigars.Add(cigar.Name);
                    importedCount++;
                }

                // Изображение: те же ключи хранилища, что при загрузке из UI (SaveAsync + миниатюра).
                if (!string.IsNullOrEmpty(cigarData.ImageUrl))
                {
                    try
                    {
                        var imageResult = await DownloadImageAsync(cigarData.ImageUrl);
                        if (imageResult != null)
                        {
                            var cigarImage = new CigarImage
                            {
                                CigarBase = cigar,
                                FileName = importImageFileName,
                                ContentType = imageResult.ContentType,
                                IsMain = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            await CigarImageStorageWriter.WriteOriginalAndThumbnailAsync(
                                cigarImage,
                                imageResult.Data,
                                _imageStorage,
                                _thumbnails,
                                _imageStorageOptions,
                                CancellationToken.None);

                            _context.CigarImages.Add(cigarImage);
                            imagesCount++;
                        }
                        else
                        {
                            imagesErrorCount++;
                            Console.WriteLine($"Не удалось загрузить изображение для {cigar.Name}: {cigarData.ImageUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        imagesErrorCount++;
                        Console.WriteLine($"Ошибка при загрузке изображения для {cigar.Name}: {ex.Message}");
                    }
                }

                if (progressTick % 50 == 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Импортировано {importedCount} сигар, {imagesCount} изображений...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка импорта сигары {cigarData.Name}: {ex.Message}");
                skippedCount++;
            }
        }
        
        await _context.SaveChangesAsync();
        Console.WriteLine(
            $"Импортировано: {importedCount} сигар, {imagesCount} изображений, " +
            $"Пропущено: {skippedCount}, Ошибок изображений: {imagesErrorCount}");
    }
    
    private async Task<ImageDownloadResult?> DownloadImageAsync(string imageUrl)
    {
        try
        {
            // Проверяем URL на валидность
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) || uri is null)
            {
                _logger.LogError($"Неверный URL изображения: {imageUrl}");
                return null;
            }

            // Скачиваем изображение
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Не удалось загрузить изображение с {imageUrl}, код статуса: {response.StatusCode}");
                return null;
            }
            
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            if (!contentType.StartsWith("image/"))
            {
                _logger.LogError($"URL {imageUrl} не является изображением, тип контента: {contentType}");
                return null;
            }
            
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            if (imageBytes == null || imageBytes.Length == 0)
            {
                _logger.LogError($"Получено пустое изображение с {imageUrl}");
                return null;
            }
            
            return new ImageDownloadResult
            {
                ContentType = contentType,
                Data = imageBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при скачивании изображения с {imageUrl}: {ex.Message}");
            return null;
        }
    }
    
    private string GetFileNameFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri is not null)
        {
            var fileName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
        }
        
        return Guid.NewGuid().ToString().Substring(0, 8) + ".jpg";
    }
    
    private string GetContentTypeFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri is not null)
        {
            var extension = Path.GetExtension(uri.LocalPath).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
                case ".avif":
                    return "image/avif";
            }
        }
        
        return "image/jpeg"; // По умолчанию
    }

    private string ExtractBrandName(string cigarName)
    {
        // Извлекаем название бренда из названия сигары
        var parts = cigarName.Split(' ');
        if (parts.Length >= 2)
        {
            // Кубинские бренды (составные названия)
            if (parts[0].ToLower() == "hoyo" && parts[1].ToLower() == "de")
                return "Hoyo De Monterrey";
            if (parts[0].ToLower() == "romeo" && parts[1].ToLower() == "y")
                return "Romeo Y Julieta";
            if (parts[0].ToLower() == "h." && parts[1].ToLower() == "upmann")
                return "H.Upmann";
            if (parts[0].ToLower() == "quai" && parts[1].ToLower() == "d'orsay")
                return "Quai d'Orsay";
            if (parts[0].ToLower() == "s." && parts[1].ToLower() == "cristobal")
                return "S.Cristobal";
            if (parts[0].ToLower() == "zino" && parts[1].ToLower() == "platinum")
                return "Zino Platinum";
            if (parts[0].ToLower() == "casa" && parts[1].ToLower() == "turrent")
                return "Casa Turrent";
            if (parts[0].ToLower() == "cuesta" && parts[1].ToLower() == "rey")
                return "Cuesta Rey";
            if (parts[0].ToLower() == "por" && parts[1].ToLower() == "larranaga")
                return "Por Larranaga";
            if (parts[0].ToLower() == "ramon" && parts[1].ToLower() == "allones")
                return "Ramon Allones";
            if (parts[0].ToLower() == "rey" && parts[1].ToLower() == "del")
                return "Rey Del Mundo";
            if (parts[0].ToLower() == "siglo" && parts[1].ToLower() == "de")
                return "Siglo de Oro";
            if (parts[0].ToLower() == "vegas" && parts[1].ToLower() == "robaina")
                return "Vegas Robaina";
            if (parts[0].ToLower() == "juan" && parts[1].ToLower() == "lopez")
                return "Juan Lopez";
            if (parts[0].ToLower() == "aj" && parts[1].ToLower() == "fernandez")
                return "AJ Fernandez";
            if (parts[0].ToLower() == "evgenij" && parts[1].ToLower() == "onegin")
                return "Евгений Онегин";
            if (parts[0].ToLower() == "pelo" && parts[1].ToLower() == "de")
                return "Pelo de Oro";
            if (parts[0].ToLower() == "don" && parts[1].ToLower() == "tomas")
                return "Don Tomas";
            if (parts[0].ToLower() == "jose" && parts[1].ToLower() == "l.piedra")
                return "Jose L.Piedra";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "aroma")
                return "La Aroma Del Caribe";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "aurora")
                return "La Aurora";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "estrella")
                return "La Estrella";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "flor" && parts.Length >= 3)
            {
                // Различаем "La Flor De Cano" и "La Flor Dominicana"
                if (parts[2].ToLower() == "de")
                    return "La Flor De Cano";
                if (parts[2].ToLower() == "dominicana")
                    return "La Flor Dominicana";
                return "La Flor De Cano"; // По умолчанию
            }
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "aroma" && parts.Length >= 4)
            {
                // "La Aroma Del Caribe"
                if (parts[2].ToLower() == "del" && parts[3].ToLower() == "caribe")
                    return "La Aroma Del Caribe";
            }
            if (parts[0].ToLower() == "hiram" && parts[1].ToLower() == "solomon" && parts.Length >= 3)
            {
                // "Hiram Solomon Cigars"
                if (parts[2].ToLower() == "cigars")
                    return "Hiram Solomon Cigars";
            }
            if (parts[0].ToLower() == "jm" && parts[1].ToLower() == "tobacco" && parts.Length >= 3)
            {
                // "JM Tobacco Cigars"
                if (parts[2].ToLower() == "cigars")
                    return "JM Tobacco Cigars";
            }
            if (parts[0].ToLower() == "tatuaje" && parts[1].ToLower() == "cigars")
                return "Tatuaje Cigars";
            if (parts[0].ToLower() == "total" && parts[1].ToLower() == "flame")
                return "Total Flame";
            if (parts[0].ToLower() == "villa" && parts[1].ToLower() == "vieja")
                return "Villa Vieja";
            if (parts[0].ToLower() == "villa" && parts[1].ToLower() == "zamorano")
                return "Villa Zamorano";
            if (parts[0].ToLower() == "arturo" && parts[1].ToLower() == "fuente")
                return "Arturo Fuente";
            if (parts[0].ToLower() == "diamond" && parts[1].ToLower() == "crown")
                return "Diamond Crown";
            if (parts[0].ToLower() == "dominican" && parts[1].ToLower() == "estates")
                return "Dominican Estates";
            if (parts[0].ToLower() == "don" && parts[1].ToLower() == "diego")
                return "Don Diego";
            if (parts[0].ToLower() == "drew" && parts[1].ToLower() == "estate")
                return "Drew Estate";
            if (parts[0].ToLower() == "el" && parts[1].ToLower() == "baton")
                return "El Baton";
            if (parts[0].ToLower() == "ernesto" && parts[1].ToLower() == "perez")
                return "Ernesto Perez";
            if (parts[0].ToLower() == "flor" && parts[1].ToLower() == "de")
                return "Flor De Copan";
            if (parts[0].ToLower() == "god" && parts[1].ToLower() == "of")
                return "God Of Fire";
            if (parts[0].ToLower() == "great" && parts[1].ToLower() == "wall")
                return "Great Wall";
            if (parts[0].ToLower() == "perla" && parts[1].ToLower() == "del")
                return "Perla Del Mar";
            if (parts[0].ToLower() == "saint" && parts[1].ToLower() == "luis")
                return "Saint Luis Rey";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "galera")
                return "La Galera";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "gloria")
                return "La Gloria Cubana";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "instructora")
                return "La Instructora";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "ley")
                return "La Ley";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "unica")
                return "La Unica";
            if (parts[0].ToLower() == "leon" && parts[1].ToLower() == "jimenes")
                return "Leon Jimenes";
            if (parts[0].ToLower() == "luis" && parts[1].ToLower() == "martinez")
                return "Luis Martinez";
            if (parts[0].ToLower() == "hiram" && parts[1].ToLower() == "solomon")
                return "Hiram Solomon Cigars";
            if (parts[0].ToLower() == "jm" && parts[1].ToLower() == "tobacco")
                return "JM Tobacco Cigars";
            if (parts[0].ToLower() == "joya" && parts[1].ToLower() == "de")
                return "Joya De Nicaragua";
            if (parts[0].ToLower() == "my" && parts[1].ToLower() == "father")
                return "My Father";
            if (parts[0].ToLower() == "rocky" && parts[1].ToLower() == "patel")
                return "Rocky Patel";
            if (parts[0].ToLower() == "san" && parts[1].ToLower() == "lotano")
                return "San Lotano";
            if (parts[0].ToLower() == "sancho" && parts[1].ToLower() == "panza")
                return "Sancho Panza";
            if (parts[0].ToLower() == "santa" && parts[1].ToLower() == "damiana")
                return "Santa Damiana";
            if (parts[0].ToLower() == "tatuaje" && parts[1].ToLower() == "cigars")
                return "Tatuaje Cigars";
            if (parts[0].ToLower() == "te" && parts[1].ToLower() == "amo")
                return "Te Amo";
            if (parts[0].ToLower() == "total" && parts[1].ToLower() == "flame")
                return "Total Flame";
            if (parts[0].ToLower() == "villa" && parts[1].ToLower() == "vieja")
                return "Villa Vieja";
            if (parts[0].ToLower() == "villa" && parts[1].ToLower() == "zamorano")
                return "Villa Zamorano";
            if (parts[0].ToLower() == "arturo" && parts[1].ToLower() == "fuente")
                return "Arturo Fuente";
            if (parts[0].ToLower() == "diamond" && parts[1].ToLower() == "crown")
                return "Diamond Crown";
            if (parts[0].ToLower() == "dominican" && parts[1].ToLower() == "estates")
                return "Dominican Estates";
            if (parts[0].ToLower() == "don" && parts[1].ToLower() == "diego")
                return "Don Diego";
            if (parts[0].ToLower() == "drew" && parts[1].ToLower() == "estate")
                return "Drew Estate";
            if (parts[0].ToLower() == "el" && parts[1].ToLower() == "baton")
                return "El Baton";
            if (parts[0].ToLower() == "ernesto" && parts[1].ToLower() == "perez")
                return "Ernesto Perez";
            if (parts[0].ToLower() == "flor" && parts[1].ToLower() == "de")
                return "Flor De Copan";
            if (parts[0].ToLower() == "god" && parts[1].ToLower() == "of")
                return "God Of Fire";
            if (parts[0].ToLower() == "great" && parts[1].ToLower() == "wall")
                return "Great Wall";
            if (parts[0].ToLower() == "la" && parts[1].ToLower() == "flor")
                return "La Flor Dominicana";
            if (parts[0].ToLower() == "perla" && parts[1].ToLower() == "del")
                return "Perla Del Mar";
            if (parts[0].ToLower() == "saint" && parts[1].ToLower() == "luis")
                return "Saint Luis Rey";
            
            // Одиночные бренды (первое слово)
            var firstWord = parts[0].ToLower();
            switch (firstWord)
            {
                // Кубинские бренды
                case "cohiba": return "Cohiba";
                case "montecristo": return "Montecristo";
                case "partagas": return "Partagas";
                case "trinidad": return "Trinidad";
                case "bolivar": return "Bolivar";
                case "diplomaticos": return "Diplomaticos";
                case "fonseca": return "Fonseca";
                case "quintero": return "Quintero";
                case "orishas": return "ORISHAS";
                case "hidalgo": return "Hidalgo";
                case "punch": return "Punch";
                case "paradiso": return "Paradiso";
                case "vegueros": return "Vegueros";
                
                // Международные бренды
                case "aj": return "AJ Fernandez";
                case "alec": return "Alec Bradley";
                case "alfambra": return "Alfambra";
                case "ararat": return "Ararat";
                case "aristocrat": return "Aristocrat";
                case "ashton": return "Ashton";
                case "asylum": return "Asylum";
                case "atabey": return "Atabey";
                case "avo": return "AVO";
                case "balmoral": return "Balmoral";
                case "bentley": return "Bentley";
                case "bossner": return "Bossner";
                case "brick": return "Brick House";
                case "buena": return "Buena Vista";
                case "cain": return "Cain";
                case "caldwell": return "Caldwell";
                case "camacho": return "Camacho";
                case "cao": return "CAO";
                case "capadura": return "Capadura";
                case "carlos": return "Carlos Andre";
                case "casa": return "Casa 1910";
                case "cherokee": return "Cherokee";
                case "cle": return "CLE";
                case "cuaba": return "Cuaba";
                case "cuba": return "Cuba Aliados";
                case "cumpay": return "Cumpay";
                case "cusano": return "Cusano";
                case "dardanelles": return "Dardanelles";
                case "davidoff": return "Davidoff";
                case "davtian": return "Davtian";
                case "diesel": return "Diesel";
                case "eiroa": return "Eiroa";
                case "evgenij": return "Евгений Онегин";
                case "furia": return "Furia";
                case "griffins": return "Griffins";
                case "guantanamera": return "Guantanamera";
                case "gurkha": return "Gurkha";
                case "horacio": return "Horacio";
                case "humo": return "Humo Jaguar";
                case "jose": return "Jose L.Piedra";
                case "juan": return "Juan Lopez";
                case "leon": return "Leon Jimenes";
                case "luis": return "Luis Martinez";
                case "macanudo": return "Macanudo";
                case "miro": return "Miro";
                case "montosa": return "Montosa";
                case "nicarao": return "Nicarao";
                case "nub": return "NUB";
                case "oliva": return "Oliva";
                case "other": return "Other";
                case "padron": return "Padron";
                case "parcero": return "Parcero";
                case "pdr": return "PDR";
                case "pelo": return "Pelo de Oro";
                case "perdomo": return "Perdomo";
                case "plasensia": return "Plasensia";
                case "pogarskaya": return "Pogarskaya Fabrika";
                case "por": return "Por Larranaga";
                case "ramon": return "Ramon Allones";
                case "rey": return "Rey Del Mundo";
                case "rocky": return "Rocky Patel";
                case "romeo": return "Romeo Y Julieta";
                case "s": return "S.Cristobal";
                case "samana": return "Samana";
                case "san": return "San Lotano";
                case "sancho": return "Sancho Panza";
                case "santa": return "Santa Damiana";
                case "sicario": return "Sicario";
                case "siglo": return "Siglo de Oro";
                case "stanislaw": return "Stanislaw";
                case "te": return "Te Amo";
                case "toreo": return "Toreo";
                case "torres": return "Torres";
                case "toscano": return "Toscano";
                case "vegas": return "Vegas Robaina";
                case "vegafina": return "Vegafina";
                case "vintage": return "Vintage";
                case "xo": return "XO";
                case "yoruba": return "Yoruba";
                case "zapata": return "Zapata";
                case "zino": return "Zino";
            }
            
            return parts[0];
        }
        
        return cigarName;
    }

    /// <summary>
    /// Заполняет <see cref="CigarBase.LengthMm"/> и <see cref="CigarBase.Diameter"/> из поля CSV:
    /// «130 мм × 55 RG» или строка вида «AAA x BB» / «AAA×BB».
    /// </summary>
    private static void ApplyVitolaFromCsvField(CigarBase cigar, string? sizeText)
    {
        if (string.IsNullOrWhiteSpace(sizeText))
            return;

        var t = sizeText.Trim();
        var mmRg = Regex.Match(t, @"(\d+)\s*мм\s*[×xX]\s*(\d+)\s*RG", RegexOptions.IgnoreCase);
        if (mmRg.Success)
        {
            cigar.LengthMm = int.Parse(mmRg.Groups[1].Value, CultureInfo.InvariantCulture);
            cigar.Diameter = int.Parse(mmRg.Groups[2].Value, CultureInfo.InvariantCulture);
            return;
        }

        var pair = Regex.Match(t, @"^(\d+)\s*[xX×]\s*(\d+)$");
        if (pair.Success)
        {
            cigar.LengthMm = int.Parse(pair.Groups[1].Value, CultureInfo.InvariantCulture);
            cigar.Diameter = int.Parse(pair.Groups[2].Value, CultureInfo.InvariantCulture);
        }
    }
}

public class CigarData
{
    public string Url { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
}

public class ImageDownloadResult
{
    public string ContentType { get; set; } = "image/jpeg";
    public byte[] Data { get; set; } = Array.Empty<byte>();
} 