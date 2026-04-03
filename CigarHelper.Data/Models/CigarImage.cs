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

    /// <summary>
    /// Ключ оригинала во внешнем хранилище: имя объекта MinIO или имя файла в корне <c>ImageStorage:LocalPath</c>
    /// (формат <c>{guid}_{fileName}</c>, как при загрузке через API и при CSV-импорте).
    /// </summary>
    [MaxLength(500)]
    public string? StoragePath { get; set; }

    /// <summary>Ключ миниатюры WebP в том же хранилище (генерация и именование как при загрузке через API).</summary>
    [MaxLength(500)]
    public string? ThumbnailPath { get; set; }

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
