using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class CigarBriefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Strength { get; set; }
    public decimal? Price { get; set; }
    public int? Rating { get; set; }
    public string? ImageUrl { get; set; }
}

public class CigarResponseDto : CigarBriefDto
{
    public string? Country { get; set; }
    public string? Description { get; set; }
    public int? HumidorId { get; set; }
    public string? HumidorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CigarCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Strength { get; set; }
    
    [MaxLength(50)]
    public string? Size { get; set; }
    
    [Range(0, 10000)]
    public decimal? Price { get; set; }
    
    [Range(1, 10)]
    public int? Rating { get; set; }
    
    [MaxLength(255)]
    public string? ImageUrl { get; set; }
    
    public int? HumidorId { get; set; }
}

public class CigarUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Strength { get; set; }
    
    [MaxLength(50)]
    public string? Size { get; set; }
    
    [Range(0, 10000)]
    public decimal? Price { get; set; }
    
    [Range(1, 10)]
    public int? Rating { get; set; }
    
    [MaxLength(255)]
    public string? ImageUrl { get; set; }
} 