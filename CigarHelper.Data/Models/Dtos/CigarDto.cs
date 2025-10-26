using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class CigarBriefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BrandDto Brand { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Strength { get; set; }
    public decimal? Price { get; set; }
    public int? Rating { get; set; }
    public string? ImageUrl { get; set; }
}

public class CigarBaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BrandDto Brand { get; set; } = new BrandDto();
    public string? Size { get; set; }
    public string? Strength { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? Wrapper { get; set; }
    public string? Binder { get; set; }
    public string? Filler { get; set; }
    public List<CigarImageDto>? Images { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CigarResponseDto : CigarBriefDto
{
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? Wrapper { get; set; }
    public string? Binder { get; set; }
    public string? Filler { get; set; }
    public int? HumidorId { get; set; }
    public HumidorDto? Humidor { get; set; }
    public int UserId { get; set; }
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

public class UserCigarUpdateRequest
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

    public decimal? Price { get; set; }

    public int? Rating { get; set; }

    public int? HumidorId { get; set; }
}

public class CreateCigarBaseRequest
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

    public List<string>? ImageUrls { get; set; }
}

public class UpdateCigarBaseRequest
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

    public List<string>? ImageUrlsToAdd { get; set; }

    public List<int>? ImageIdsToRemove { get; set; }

    public int? MainImageId { get; set; }
}