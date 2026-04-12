using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

/// <summary>Добавление сигары в коллекцию по существующей записи справочника (CigarBase), в т.ч. до модерации карточки.</summary>
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

    /// <summary>Количество сигар (шт.); null или вне диапазона — на сервере сохраняется как 1.</summary>
    [Range(1, 9999)]
    public int? Quantity { get; set; }

    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    /// <summary>Несколько URL — скачиваются по порядку; первое успешно загруженное становится главным.</summary>
    public List<string>? ImageUrls { get; set; }
}
