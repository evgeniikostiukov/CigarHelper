using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class CreateCigarRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int BrandId { get; set; }
    
    public string? Country { get; set; }
    
    public string? Description { get; set; }
    
    public string? Strength { get; set; }
    
    public string? Size { get; set; }
    
    public string? Wrapper { get; set; }
    
    public string? Binder { get; set; }
    
    public string? Filler { get; set; }
    
    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    /// <summary>Несколько URL — скачиваются по порядку; первое успешно загруженное становится главным. Если задано, <see cref="ImageUrl"/> игнорируется.</summary>
    public List<string>? ImageUrls { get; set; }

    public decimal? Price { get; set; }
    
    public int? Rating { get; set; }
    
    public int? HumidorId { get; set; }
} 