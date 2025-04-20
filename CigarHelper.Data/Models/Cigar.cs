using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("Cigars")]
public class Cigar
{
    [Key]
    public int Id { get; set; }
    
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
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Price { get; set; }
    
    public int? Rating { get; set; }
    
    [MaxLength(255)]
    public string? ImageUrl { get; set; }
    
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    public int? HumidorId { get; set; }
    
    [ForeignKey("HumidorId")]
    public Humidor? Humidor { get; set; }
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 