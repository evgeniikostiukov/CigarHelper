using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("ReviewImages")]
public class ReviewImage
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public byte[]? ImageBytes { get; set; } = [];
    
    [MaxLength(500)]
    public string? Caption { get; set; }
    
    public int ReviewId { get; set; }
    
    [ForeignKey("ReviewId")]
    public Review Review { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 