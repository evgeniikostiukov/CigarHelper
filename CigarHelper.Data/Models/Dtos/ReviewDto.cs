using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class ReviewDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public int CigarId { get; set; }
    public string CigarName { get; set; } = string.Empty;
    public string CigarBrand { get; set; } = string.Empty;
    public List<ReviewImageDto> Images { get; set; } = new List<ReviewImageDto>();
    public string? SmokingExperience { get; set; }
    public string? Aroma { get; set; }
    public string? Taste { get; set; }
    public string? Construction { get; set; }
    public string? BurnQuality { get; set; }
    public string? Draw { get; set; }
    public string? Venue { get; set; }
    public DateTime SmokingDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReviewListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty; // Короткая версия контента
    public int Rating { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public int CigarId { get; set; }
    public string CigarName { get; set; } = string.Empty;
    public string CigarBrand { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; } // Главное изображение обзора
    public int ImageCount { get; set; } // Количество изображений
    public DateTime CreatedAt { get; set; }
}

public class ReviewImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
}

public class CreateReviewRequest
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Заголовок должен быть от 3 до 200 символов")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Содержание обзора обязательно")]
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Оценка обязательна")]
    [Range(1, 10, ErrorMessage = "Оценка должна быть от 1 до 10")]
    public int Rating { get; set; }
    
    [Required(ErrorMessage = "Идентификатор сигары обязателен")]
    public int CigarId { get; set; }
    
    public string? SmokingExperience { get; set; }
    public string? Aroma { get; set; }
    public string? Taste { get; set; }
    public string? Construction { get; set; }
    public string? BurnQuality { get; set; }
    public string? Draw { get; set; }
    public string? Venue { get; set; }
    public DateTime? SmokingDate { get; set; }
    
    public List<CreateReviewImageRequest> Images { get; set; } = new List<CreateReviewImageRequest>();
}

public class CreateReviewImageRequest
{
    [Required(ErrorMessage = "URL изображения обязателен")]
    public string ImageUrl { get; set; } = string.Empty;
    
    public string? Caption { get; set; }
}

public class UpdateReviewRequest
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Заголовок должен быть от 3 до 200 символов")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Содержание обзора обязательно")]
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Оценка обязательна")]
    [Range(1, 10, ErrorMessage = "Оценка должна быть от 1 до 10")]
    public int Rating { get; set; }
    
    public string? SmokingExperience { get; set; }
    public string? Aroma { get; set; }
    public string? Taste { get; set; }
    public string? Construction { get; set; }
    public string? BurnQuality { get; set; }
    public string? Draw { get; set; }
    public string? Venue { get; set; }
    public DateTime? SmokingDate { get; set; }
    
    // Список изображений для добавления
    public List<CreateReviewImageRequest>? ImagesToAdd { get; set; }
    
    // Список идентификаторов изображений для удаления
    public List<int>? ImageIdsToRemove { get; set; }
} 