using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Data.Models;

/// <summary>Комментарий пользователя к публичному обзору (<see cref="Review"/>).</summary>
[Table("ReviewComments")]
public class ReviewComment
{
    [Key]
    public int Id { get; set; }

    public int ReviewId { get; set; }

    [ForeignKey(nameof(ReviewId))]
    public Review Review { get; set; } = null!;

    public int AuthorUserId { get; set; }

    [ForeignKey(nameof(AuthorUserId))]
    public User Author { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CigarCommentModerationStatus ModerationStatus { get; set; } = CigarCommentModerationStatus.Approved;

    public DateTime? ModeratedAt { get; set; }

    public int? ModeratedByUserId { get; set; }

    [ForeignKey(nameof(ModeratedByUserId))]
    public User? ModeratedBy { get; set; }
}
