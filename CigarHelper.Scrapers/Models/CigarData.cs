using System.Text.Json.Serialization;

namespace CigarHelper.Scrapers.Models;

public class CigarData
{
    public List<CigarBrand> Brands { get; set; } = new();
}

public class CigarBrand
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CigarProduct> Products { get; set; } = new();
}

public class CigarProduct
{
    public string Name { get; set; } = string.Empty;
    public string Vitola { get; set; } = string.Empty;
    [JsonPropertyName("length")]
    public float? Length { get; set; }
    [JsonPropertyName("ringGauge")]
    public int? RingGauge { get; set; }
    public string Strength { get; set; } = string.Empty;
    public string Wrapper { get; set; } = string.Empty;
    public string Binder { get; set; } = string.Empty;
    public string Filler { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal? Price { get; set; }
} 