using System.Net;
using System.Text.RegularExpressions;
using CigarHelper.Scrapers.Models;
using HtmlAgilityPack;

namespace CigarHelper.Scrapers.Services;

public class CigarDayScraperService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://cigarday.ru";
    
    public CigarDayScraperService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
    }
    
    public async Task<CigarData> ScrapeCigarsAsync()
    {
        var result = new CigarData();
        
        try
        {
            // Get main catalog page
            var catalogUrl = $"{_baseUrl}/catalog/cigars";
            var html = await _httpClient.GetStringAsync(catalogUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            // Get all brand links from the catalog sidebar
            var brandNodes = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'catalog__sidebar-menu')]//a[contains(@href, 'cigars/')]");
            
            if (brandNodes != null)
            {
                foreach (var brandNode in brandNodes)
                {
                    var brandUrl = brandNode.GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(brandUrl)) continue;
                    
                    // Skip non-brand links
                    if (!brandUrl.Contains("/cigars/")) continue;
                    
                    Console.WriteLine($"Processing brand: {brandNode.InnerText.Trim()}");
                    
                    var brandData = await ScrapeBrandAsync($"{_baseUrl}{brandUrl}");
                    if (brandData != null)
                    {
                        result.Brands.Add(brandData);
                    }
                    
                    // Avoid hitting the server too hard
                    await Task.Delay(1000);
                }
            }
            else
            {
                Console.WriteLine("No brand links found. The site structure might have changed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping cigars: {ex.Message}");
        }
        
        return result;
    }
    
    private async Task<CigarBrand?> ScrapeBrandAsync(string brandUrl)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(brandUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            // Extract brand name
            var brandNameNode = doc.DocumentNode.SelectSingleNode("//h1[@class='catalog-section__title']");
            if (brandNameNode == null) return null;
            
            var brandName = brandNameNode.InnerText.Trim();
            Console.WriteLine($"Brand name: {brandName}");
            
            var brand = new CigarBrand
            {
                Name = brandName,
                Country = GetBrandCountry(doc),
                Description = GetBrandDescription(doc),
                Products = new List<CigarProduct>()
            };
            
            // Get all product items
            var productNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'catalog-section__item')]");
            if (productNodes != null)
            {
                foreach (var productNode in productNodes)
                {
                    var productUrl = productNode.SelectSingleNode(".//a[contains(@class, 'catalog-section__item-title')]")?.GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(productUrl)) continue;
                    
                    Console.WriteLine($"Processing product: {productNode.SelectSingleNode(".//a[contains(@class, 'catalog-section__item-title')]")?.InnerText.Trim()}");
                    
                    var productData = await ScrapeProductAsync($"{_baseUrl}{productUrl}");
                    if (productData != null)
                    {
                        brand.Products.Add(productData);
                    }
                    
                    // Avoid hitting the server too hard
                    await Task.Delay(1000);
                }
            }
            
            return brand;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping brand {brandUrl}: {ex.Message}");
            return null;
        }
    }
    
    private string GetBrandCountry(HtmlDocument doc)
    {
        // Try to find country information from the brand page
        var countryNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product__info-row') and contains(., 'Страна')]/div[@class='product__info-value']");
        return countryNode?.InnerText.Trim() ?? string.Empty;
    }
    
    private string GetBrandDescription(HtmlDocument doc)
    {
        // Try to find description information
        var descNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'catalog-section__description')]");
        return descNode?.InnerText.Trim() ?? string.Empty;
    }
    
    private async Task<CigarProduct?> ScrapeProductAsync(string productUrl)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(productUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            var product = new CigarProduct();
            
            // Extract product name
            var nameNode = doc.DocumentNode.SelectSingleNode("//h1[@class='product__title']");
            if (nameNode != null)
            {
                product.Name = nameNode.InnerText.Trim();
            }
            
            // Extract product details
            ExtractProductDetails(doc, product);
            
            // Get product description
            var descNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product__description')]");
            if (descNode != null)
            {
                product.Description = WebUtility.HtmlDecode(descNode.InnerText.Trim());
            }
            
            // Get product image URL
            var imageNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product__photo')]/img");
            if (imageNode != null)
            {
                var imageSrc = imageNode.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(imageSrc))
                {
                    product.ImageUrl = imageSrc.StartsWith("http") ? imageSrc : $"{_baseUrl}{imageSrc}";
                }
            }
            
            // Get product price
            var priceNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product__price')]");
            if (priceNode != null)
            {
                var priceText = priceNode.InnerText.Trim();
                var priceMatch = Regex.Match(priceText, @"(\d+[\s\d]*,\d+|\d+[\s\d]*)");
                if (priceMatch.Success)
                {
                    string cleanPrice = priceMatch.Groups[1].Value.Replace(" ", "").Replace(",", ".");
                    if (decimal.TryParse(cleanPrice, out var price))
                    {
                        product.Price = price;
                    }
                }
            }
            
            return product;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping product {productUrl}: {ex.Message}");
            return null;
        }
    }
    
    private void ExtractProductDetails(HtmlDocument doc, CigarProduct product)
    {
        var infoRows = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product__info-row')]");
        if (infoRows == null) return;
        
        foreach (var row in infoRows)
        {
            var titleNode = row.SelectSingleNode(".//div[contains(@class, 'product__info-title')]");
            var valueNode = row.SelectSingleNode(".//div[contains(@class, 'product__info-value')]");
            
            if (titleNode == null || valueNode == null) continue;
            
            var title = titleNode.InnerText.Trim().ToLower();
            var value = valueNode.InnerText.Trim();
            
            switch (title)
            {
                case "формат":
                case "витола":
                    product.Vitola = value;
                    break;
                case "крепость":
                    product.Strength = value;
                    break;
                case "покровный лист":
                    product.Wrapper = value;
                    break;
                case "связующий лист":
                    product.Binder = value;
                    break;
                case "наполнитель":
                    product.Filler = value;
                    break;
                case "длина":
                    if (float.TryParse(value.Replace(" см", "").Replace(",", "."), out var length))
                    {
                        product.Length = length;
                    }
                    break;
                case "диаметр":
                case "ринг":
                case "ring":
                    var ringMatch = Regex.Match(value, @"(\d+)");
                    if (ringMatch.Success && int.TryParse(ringMatch.Groups[1].Value, out var ring))
                    {
                        product.RingGauge = ring;
                    }
                    break;
            }
        }
    }
} 