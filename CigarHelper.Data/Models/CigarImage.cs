using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("CigarImages")]
public class CigarImage
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(255)]
    public string? FileName { get; set; }
    
    [MaxLength(100)]
    public string? ContentType { get; set; }
    
    public long? FileSize { get; set; }
    
    [Column(TypeName = "bytea")]
    public byte[]? ImageData { get; set; }
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    public bool IsMain { get; set; } = false;
    
    public int? CigarBaseId { get; set; }
    
    [ForeignKey("CigarBaseId")]
    public CigarBase? CigarBase { get; set; }
    
    public int? UserCigarId { get; set; }
    
    [ForeignKey("UserCigarId")]
    public UserCigar? UserCigar { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 