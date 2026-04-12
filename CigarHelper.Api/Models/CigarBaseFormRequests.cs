using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Api.Models;

/// <summary>Один загружаемый файл изображения в составе multipart-запроса.</summary>
public class NewImageUpload
{
    public IFormFile? File { get; set; }
    public bool IsMain { get; set; }
}

/// <summary>Обновление флага IsMain для существующего изображения.</summary>
public class ExistingImageUpdate
{
    public int Id { get; set; }
    public bool IsMain { get; set; }
}

/// <summary>Создание CigarBase с загрузкой файлов (multipart/form-data).</summary>
public class CreateCigarBaseFormRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public int BrandId { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? Strength { get; set; }
    public string? Size { get; set; }
    public string? Wrapper { get; set; }
    public string? Binder { get; set; }
    public string? Filler { get; set; }

    /// <summary>Загружаемые файлы изображений.</summary>
    public List<NewImageUpload>? NewImages { get; set; }

    /// <summary>URL изображений (http/https); скачиваются на сервере и привязываются к карточке после <see cref="NewImages"/>.</summary>
    public List<string>? ImageUrls { get; set; }

    /// <summary>Главный кадр по индексу в <see cref="ImageUrls"/> (длина может быть меньше списка URL).</summary>
    public List<bool>? ImageUrlIsMain { get; set; }
}

/// <summary>Обновление CigarBase с загрузкой файлов (multipart/form-data).</summary>
public class UpdateCigarBaseFormRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public int BrandId { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? Strength { get; set; }
    public string? Size { get; set; }
    public string? Wrapper { get; set; }
    public string? Binder { get; set; }
    public string? Filler { get; set; }

    /// <summary>Новые загружаемые файлы изображений.</summary>
    public List<NewImageUpload>? NewImages { get; set; }

    /// <summary>Обновление IsMain для существующих изображений.</summary>
    public List<ExistingImageUpdate>? ExistingImages { get; set; }

    /// <summary>ID изображений для удаления (имя соответствует полю FormData на фронте).</summary>
    public List<int>? ImageIdsToDelete { get; set; }
}
