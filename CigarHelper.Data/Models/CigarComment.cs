using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CigarHelper.Data.Models;

/// <summary>
/// Короткий комментарий другого пользователя к записи справочника (CigarBase) или к экземпляру в коллекции (UserCigar).
/// Ровно одно из <see cref="CigarBaseId"/> / <see cref="UserCigarId"/> задано.
/// </summary>
[Table("CigarComments")]
public class CigarComment
{
    [Key]
    public int Id { get; set; }

    public int AuthorUserId { get; set; }

    [ForeignKey(nameof(AuthorUserId))]
    public User Author { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CigarBaseId { get; set; }

    [ForeignKey(nameof(CigarBaseId))]
    public CigarBase? CigarBase { get; set; }

    public int? UserCigarId { get; set; }

    [ForeignKey(nameof(UserCigarId))]
    public UserCigar? UserCigar { get; set; }
}
