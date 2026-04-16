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

    /// <summary>Каталожная сигара — всегда задана.</summary>
    public int CigarBaseId { get; set; }

    [ForeignKey("CigarBaseId")]
    public CigarBase CigarBase { get; set; } = null!;

    /// <summary>Опционально: запись коллекции пользователя, если обзор привязан к «моей» сигаре.</summary>
    public int? CigarId { get; set; }

    [ForeignKey("CigarId")]
    public UserCigar? Cigar { get; set; }
    
    public ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();

    public ICollection<ReviewComment> Comments { get; set; } = new List<ReviewComment>();
    
    [MaxLength(50)]
    public string? SmokingExperience { get; set; }
    
    [MaxLength(50)]
    public string? Aroma { get; set; }
    
    [MaxLength(50)]
    public string? Taste { get; set; }

    /// <summary>Субъективная сила/тело (1–10), для агрегатов в каталоге; не путать с каталожной крепостью.</summary>
    [Range(1, 10)]
    public int? BodyStrengthScore { get; set; }

    /// <summary>Числовая ось аромата (1–10); строковое <see cref="Aroma"/> — текстовая заметка.</summary>
    [Range(1, 10)]
    public int? AromaScore { get; set; }

    /// <summary>Сочетания (напитки/еда), субъективная ось 1–10.</summary>
    [Range(1, 10)]
    public int? PairingsScore { get; set; }

    [Range(1, 5)]
    public int? Construction { get; set; }

    [Range(1, 5)]
    public int? BurnQuality { get; set; }

    [Range(1, 5)]
    public int? Draw { get; set; }
    
    [MaxLength(100)]
    public string? Venue { get; set; }
    
    public DateTime SmokingDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Мягкое удаление автором; null — обзор активен.</summary>
    public DateTime? DeletedAt { get; set; }
} 