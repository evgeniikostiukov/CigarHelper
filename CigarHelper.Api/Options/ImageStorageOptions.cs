namespace CigarHelper.Api.Options;

/// <summary>
/// Конфигурация хранилища изображений (секция <c>ImageStorage</c>).
/// </summary>
public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>
    /// Провайдер хранилища: <c>Database</c> (по умолчанию) или <c>LocalFile</c>.
    /// При миграции на S3/MinIO — добавить провайдер <c>S3</c> и реализацию <c>IImageStorageProvider</c>.
    /// </summary>
    public string Provider { get; set; } = "Database";

    /// <summary>Корневая папка для LocalFile-провайдера. Может быть абсолютным или относительным путём.</summary>
    public string LocalPath { get; set; } = "uploads/images";

    /// <summary>Максимальная ширина миниатюры (px). По умолчанию 320.</summary>
    public int ThumbnailMaxWidth { get; set; } = 320;

    /// <summary>Максимальная высота миниатюры (px). По умолчанию 320.</summary>
    public int ThumbnailMaxHeight { get; set; } = 320;
}
