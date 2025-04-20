using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class HumidorCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Range(1, 1000)]
    public int Capacity { get; set; }
    
    [Range(0, 100)]
    public int? CurrentHumidity { get; set; }
    
    [Range(0, 50)]
    public decimal? CurrentTemperature { get; set; }
}

public class HumidorUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Range(1, 1000)]
    public int Capacity { get; set; }
    
    [Range(0, 100)]
    public int? CurrentHumidity { get; set; }
    
    [Range(0, 50)]
    public decimal? CurrentTemperature { get; set; }
}

public class HumidorResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public int? CurrentHumidity { get; set; }
    public decimal? CurrentTemperature { get; set; }
    public int CigarCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class HumidorDetailResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public int? CurrentHumidity { get; set; }
    public decimal? CurrentTemperature { get; set; }
    public List<CigarBriefDto> Cigars { get; set; } = new List<CigarBriefDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 