using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    
    [Required]
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    
    [MaxLength(255)]
    public string? AvatarUrl { get; set; }
    
    public ICollection<Humidor> Humidors { get; set; } = new List<Humidor>();
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLogin { get; set; }
} 