using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class CigarBriefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BrandDto Brand { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Strength { get; set; }
    public decimal? Price { get; set; }
    public int? Rating { get; set; }
    public string? ImageUrl { get; set; }
}

public class CigarBaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BrandDto Brand { get; set; } = new BrandDto();
    public string? Size { get; set; }
    public string? Strength { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? Wrapper { get; set; }
    public string? Binder { get; set; }
    public string? Filler { get; set; }
    public List<CigarImageDto>? Images { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CigarResponseDto : CigarBriefDto
{
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? Wrapper { get; set; }
    public string? Binder { get; set; }
    public string? Filler { get; set; }
    public int? HumidorId { get; set; }
    public HumidorDto? Humidor { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? SmokedAt { get; set; }
    public DateTime LastTouchedAt { get; set; }
    public bool IsSmoked => SmokedAt.HasValue;

    /// <summary>Заметки пользователя о вкусе (не путать с полями обзора).</summary>
    public string? Taste { get; set; }

    /// <summary>Заметки пользователя об аромате.</summary>
    public string? Aroma { get; set; }

    /// <summary>Количество сигар (шт.) в записи коллекции.</summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Галерея для карточки коллекции: сначала изображения личной сигары (UserCigar), затем изображения базы (CigarBase).
    /// </summary>
    public List<CigarImageDto>? Images { get; set; }
}

/// <summary>Обновление только личных полей записи в коллекции; карточка справочника (CigarBase) не меняется.</summary>
public class UserCigarUpdateRequest
{
    public decimal? Price { get; set; }

    public int? HumidorId { get; set; }

    [MaxLength(500)]
    public string? Taste { get; set; }

    [MaxLength(500)]
    public string? Aroma { get; set; }

    /// <summary>Субъективная оценка 1–10; null — снять оценку.</summary>
    [Range(1, 10)]
    public int? Rating { get; set; }

    /// <summary>Количество сигар (шт.), 1–9999.</summary>
    [Range(1, 9999)]
    public int Quantity { get; set; } = 1;

    /// <summary>Не пустой URL — скачать и заменить изображения этой сигары. Пустое/отсутствие — не менять картинки (кроме полей ниже).</summary>
    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    /// <summary>Добавить изображения по URL. Не используется вместе с непустым <see cref="ImageUrl"/> (тот полностью заменяет галерею).</summary>
    public List<string>? ImageUrlsToAdd { get; set; }

    /// <summary>Удалить изображения по id (только привязанные к этой UserCigar).</summary>
    public List<int>? ImageIdsToRemove { get; set; }
}

public class MarkCigarSmokedRequest
{
    public DateTime? SmokedAt { get; set; }
}

public class CreateCigarBaseRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int BrandId { get; set; }

    public string? Country { get; set; }

    public string? Description { get; set; }

    public string? Strength { get; set; }

    public string? Size { get; set; }

    public string? Wrapper { get; set; }

    public string? Binder { get; set; }

    public string? Filler { get; set; }

    public List<string>? ImageUrls { get; set; }
}

public class UpdateCigarBaseRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int BrandId { get; set; }

    public string? Country { get; set; }

    public string? Description { get; set; }

    public string? Strength { get; set; }

    public string? Size { get; set; }

    public string? Wrapper { get; set; }

    public string? Binder { get; set; }

    public string? Filler { get; set; }

    public List<string>? ImageUrlsToAdd { get; set; }

    public List<int>? ImageIdsToRemove { get; set; }

    public int? MainImageId { get; set; }
}