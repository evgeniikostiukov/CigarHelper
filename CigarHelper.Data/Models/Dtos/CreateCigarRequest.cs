using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

/// <summary>Добавление сигары в коллекцию только по записи из модерированного справочника (CigarBase).</summary>
public class CreateCigarRequest
{
    [Required]
    public int CigarBaseId { get; set; }

    public decimal? Price { get; set; }

    public int? HumidorId { get; set; }

    [MaxLength(500)]
    public string? Taste { get; set; }

    [MaxLength(500)]
    public string? Aroma { get; set; }

    /// <summary>Субъективная оценка 1–10; null — без оценки.</summary>
    [Range(1, 10)]
    public int? Rating { get; set; }

    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    /// <summary>Несколько URL — скачиваются по порядку; первое успешно загруженное становится главным.</summary>
    public List<string>? ImageUrls { get; set; }
}
