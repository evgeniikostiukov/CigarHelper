using System.ComponentModel.DataAnnotations;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Data.Models.Dtos;

public class CigarCommentDto
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AuthorUserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;

    /// <summary>Можно ли открыть публичный профиль автора.</summary>
    public bool IsAuthorProfilePublic { get; set; }

    public int? CigarBaseId { get; set; }
    public int? UserCigarId { get; set; }
    public CigarCommentModerationStatus ModerationStatus { get; set; }
}

/// <summary>Строка очереди модерации (staff).</summary>
public class AdminCigarCommentRowDto
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;

    /// <summary>Можно ли открыть публичный профиль автора.</summary>
    public bool IsAuthorProfilePublic { get; set; }

    public int? CigarBaseId { get; set; }
    public int? UserCigarId { get; set; }
    public string TargetSummary { get; set; } = string.Empty;
}

public class CreateCigarCommentRequest
{
    public int? CigarBaseId { get; set; }

    public int? UserCigarId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Body { get; set; } = string.Empty;
}
