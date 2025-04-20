using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("Humidors")]
public class Humidor
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int Capacity { get; set; }
    
    [Range(0, 100)]
    public int? CurrentHumidity { get; set; }
    
    [Range(0, 50)]
    public decimal? CurrentTemperature { get; set; }
    
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    public ICollection<Cigar> Cigars { get; set; } = new List<Cigar>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 