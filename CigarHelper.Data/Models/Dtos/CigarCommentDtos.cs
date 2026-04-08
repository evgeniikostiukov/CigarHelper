using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class CigarCommentDto
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AuthorUserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int? CigarBaseId { get; set; }
    public int? UserCigarId { get; set; }
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
