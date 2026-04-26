using System.ComponentModel.DataAnnotations;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Data.Models.Dtos;

public class ReviewCommentDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AuthorUserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public bool IsAuthorProfilePublic { get; set; }
    public CigarCommentModerationStatus ModerationStatus { get; set; }
}

public class AdminReviewCommentRowDto
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public bool IsAuthorProfilePublic { get; set; }
    public int ReviewId { get; set; }
    public string TargetSummary { get; set; } = string.Empty;
}

public class CreateReviewCommentRequest
{
    [Required]
    public int ReviewId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Body { get; set; } = string.Empty;
}
