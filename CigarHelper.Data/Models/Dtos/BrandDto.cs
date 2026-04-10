using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class BrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Description { get; set; }
    public byte[]? LogoBytes { get; set; }
    public bool IsModerated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateBrandRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(255)]
    public string? LogoUrl { get; set; }
    
    public bool IsModerated { get; set; } = false;
}

public class UpdateBrandRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(255)]
    public string? LogoUrl { get; set; }
    
    public bool IsModerated { get; set; }
} 