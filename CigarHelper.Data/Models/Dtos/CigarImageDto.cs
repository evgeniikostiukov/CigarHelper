using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class CigarImageDto
{
    public int Id { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string? Description { get; set; }
    public bool IsMain { get; set; }
    public int? CigarBaseId { get; set; }
    public int? UserCigarId { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>True если для изображения есть миниатюра (GET /{id}/thumbnail).</summary>
    public bool HasThumbnail { get; set; }
    [Obsolete("Используйте GET /api/cigar-images/{id}/data")]
    public byte[]? Data { get; set; }
}

public class CreateCigarImageRequest
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public byte[]? ImageData { get; set; }
    public string? Description { get; set; }
    public bool IsMain { get; set; } = false;
    public int? CigarBaseId { get; set; }
    public int? UserCigarId { get; set; }
}

public class UpdateCigarImageRequest
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public byte[]? ImageData { get; set; }
    public string? Description { get; set; }
    public bool? IsMain { get; set; }
}

public class UploadCigarImageByUrlRequest
{
    [Required]
    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    /// <summary>При редактировании CigarBase — сразу привязать запись изображения к базе.</summary>
    public int? CigarBaseId { get; set; }
} 