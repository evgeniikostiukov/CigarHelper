using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("Reviews")]
public class Review
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Range(1, 10)]
    public int Rating { get; set; }
    
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    public int CigarId { get; set; }
    
    [ForeignKey("CigarId")]
    public Cigar Cigar { get; set; } = null!;
    
    public ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();
    
    [MaxLength(50)]
    public string? SmokingExperience { get; set; }
    
    [MaxLength(50)]
    public string? Aroma { get; set; }
    
    [MaxLength(50)]
    public string? Taste { get; set; }
    
    [MaxLength(50)]
    public string? Construction { get; set; }
    
    [MaxLength(50)]
    public string? BurnQuality { get; set; }
    
    [MaxLength(50)]
    public string? Draw { get; set; }
    
    [MaxLength(100)]
    public string? Venue { get; set; }
    
    public DateTime SmokingDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 