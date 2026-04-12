using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("CigarBases")]
public class CigarBase
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int BrandId { get; set; }
    
    [ForeignKey("BrandId")]
    public Brand Brand { get; set; } = null!;
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Strength { get; set; }
    
    /// <summary>Длина, мм (первая часть записи «AAA × BB»).</summary>
    public int? LengthMm { get; set; }

    /// <summary>Второе число в формате «длина × кольцо» (кольцо RG).</summary>
    public int? Diameter { get; set; }
    
    [MaxLength(100)]
    public string? Wrapper { get; set; }
    
    [MaxLength(100)]
    public string? Binder { get; set; }
    
    [MaxLength(100)]
    public string? Filler { get; set; }
    
    [MaxLength(255)]
    public string? ImageUrl { get; set; }
    
    public bool IsModerated { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<UserCigar> UserCigars { get; set; } = new List<UserCigar>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public ICollection<CigarImage> Images { get; set; } = new List<CigarImage>();
} 