namespace CigarHelper.Api.Options;

/// <summary>
/// Конфигурация хранилища изображений (секция <c>ImageStorage</c>).
/// </summary>
public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>Провайдер: <c>Minio</c> (по умолчанию) или <c>LocalFile</c> (например интеграционные тесты).</summary>
    public string Provider { get; set; } = "Minio";

    /// <summary>Корневая папка для LocalFile-провайдера. Может быть абсолютным или относительным путём.</summary>
    public string LocalPath { get; set; } = "uploads/images";

    /// <summary>Максимальная ширина миниатюры (px). По умолчанию 320.</summary>
    public int ThumbnailMaxWidth { get; set; } = 320;

    /// <summary>Максимальная высота миниатюры (px). По умолчанию 320.</summary>
    public int ThumbnailMaxHeight { get; set; } = 320;

    /// <summary>Настройки MinIO-провайдера. Используется при <c>Provider = "Minio"</c>.</summary>
    public MinioOptions Minio { get; set; } = new();
}

/// <summary>
/// Настройки подключения к MinIO (или любому S3-совместимому хранилищу).
/// Чувствительные поля (AccessKey, SecretKey) задавать через User Secrets / переменные окружения.
/// </summary>
public class MinioOptions
{
    /// <summary>Адрес сервера без протокола, например <c>localhost:9000</c>.</summary>
    public string Endpoint { get; set; } = "localhost:9000";

    /// <summary>Имя бакета, в котором хранятся изображения.</summary>
    public string BucketName { get; set; } = "cigar-images";

    /// <summary>Access key (логин). Задавать через User Secrets: <c>ImageStorage:Minio:AccessKey</c>.</summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>Secret key (пароль). Задавать через User Secrets: <c>ImageStorage:Minio:SecretKey</c>.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Использовать SSL. Для локального MinIO — false.</summary>
    public bool UseSsl { get; set; } = false;
}
