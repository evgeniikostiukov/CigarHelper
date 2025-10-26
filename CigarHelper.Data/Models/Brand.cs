using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("Brands")]
public class Brand
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public byte[]? LogoBytes { get; set; }
    
    public bool IsModerated { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<CigarBase> Cigars { get; set; } = new List<CigarBase>();
} 