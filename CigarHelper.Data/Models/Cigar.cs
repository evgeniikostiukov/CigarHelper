using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("UserCigars")]
public class UserCigar
{
    [Key]
    public int Id { get; set; }
    
    public int CigarBaseId { get; set; }
    
    [ForeignKey("CigarBaseId")]
    public CigarBase CigarBase { get; set; } = null!;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Price { get; set; }
    
    public int? Rating { get; set; }
    
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    public int? HumidorId { get; set; }
    
    [ForeignKey("HumidorId")]
    public Humidor? Humidor { get; set; }
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public ICollection<CigarImage> Images { get; set; } = new List<CigarImage>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 