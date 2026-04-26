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

    /// <summary>
    /// Политика сжатия оригинала и миниатюры (WebP, лимиты сторон, параметры VP8/VP8L).
    /// Субсэмплинг хромы для lossy WebP задаётся кодеком (4:2:0); отдельное поле не требуется.
    /// </summary>
    public ImageCompressionOptions Compression { get; set; } = new();

    /// <summary>Настройки MinIO-провайдера. Используется при <c>Provider = "Minio"</c>.</summary>
    public MinioOptions Minio { get; set; } = new();

    /// <summary>Эффективные границы миниатюры: при нулевых значениях в профиле используются устаревшие поля сверху.</summary>
    public (int MaxWidth, int MaxHeight) ResolvedThumbnailBounds()
    {
        var t = Compression.Thumbnail;
        var w = t.MaxWidth > 0 ? t.MaxWidth : ThumbnailMaxWidth;
        var h = t.MaxHeight > 0 ? t.MaxHeight : ThumbnailMaxHeight;
        return (w, h);
    }

    /// <summary>Профиль миниатюры с подставленными лимитами размера.</summary>
    public StoredImageEncodingProfile ResolvedThumbnailProfile()
    {
        var t = Compression.Thumbnail;
        var (w, h) = ResolvedThumbnailBounds();
        return new StoredImageEncodingProfile
        {
            Format = t.Format,
            MaxWidth = w,
            MaxHeight = h,
            WebpQuality = t.WebpQuality,
            WebpMethod = t.WebpMethod,
            WebpLossless = t.WebpLossless,
            WebpNearLossless = t.WebpNearLossless,
            WebpNearLosslessQuality = t.WebpNearLosslessQuality,
            WebpUseAlphaCompression = t.WebpUseAlphaCompression,
            WebpEntropyPasses = t.WebpEntropyPasses,
            WebpSpatialNoiseShaping = t.WebpSpatialNoiseShaping,
            WebpFilterStrength = t.WebpFilterStrength,
            WebpSkipMetadata = t.WebpSkipMetadata,
            PreserveGifAsOriginal = t.PreserveGifAsOriginal,
            AvifCqLevel = t.AvifCqLevel,
        };
    }
}

/// <summary>Политики перекодирования для полноразмерного изображения и превью.</summary>
public class ImageCompressionOptions
{
    /// <summary>Оригинал: по умолчанию AVIF, вписывание в 2048×2048.</summary>
    public StoredImageEncodingProfile Original { get; set; } = new()
    {
        Format = "Avif",
        MaxWidth = 2048,
        MaxHeight = 2048,
        WebpQuality = 82,
        WebpMethod = 4,
        AvifCqLevel = 22,
    };

    /// <summary>Превью: WebP, 320×320 (или из <see cref="ImageStorageOptions.ThumbnailMaxWidth"/> при MaxWidth=0).</summary>
    public StoredImageEncodingProfile Thumbnail { get; set; } = new()
    {
        Format = "WebP",
        MaxWidth = 0,
        MaxHeight = 0,
        WebpQuality = 78,
        WebpMethod = 4,
    };
}

/// <summary>Параметры одного прохода resize + растровый кодек (WebP или AVIF для оригинала).</summary>
public class StoredImageEncodingProfile
{
    /// <summary>
    /// <c>WebP</c> / <c>Avif</c> — перекодировать (для оригинала); <c>KeepOriginal</c> — только для оригинала, байты как у клиента (игнорируется для миниатюры).
    /// Для миниатюры поддерживается только <c>WebP</c>.
    /// </summary>
    public string Format { get; set; } = "WebP";

    /// <summary>Максимальная ширина вписывания (0 в профиле миниатюры = взять <see cref="ImageStorageOptions.ThumbnailMaxWidth"/>).</summary>
    public int MaxWidth { get; set; }

    /// <summary>Максимальная высота вписывания (0 в профиле миниатюры = взять <see cref="ImageStorageOptions.ThumbnailMaxHeight"/>).</summary>
    public int MaxHeight { get; set; }

    /// <summary>Качество lossy WebP, 1–100.</summary>
    public int WebpQuality { get; set; } = 80;

    /// <summary>Скорость/качество кодирования WebP, 0–6 (см. <c>WebpEncodingMethod</c>).</summary>
    public int WebpMethod { get; set; } = 4;

    public bool WebpLossless { get; set; }

    public bool WebpNearLossless { get; set; }

    /// <summary>Используется при <see cref="WebpNearLossless"/>.</summary>
    public int WebpNearLosslessQuality { get; set; } = 60;

    public bool WebpUseAlphaCompression { get; set; } = true;

    public int WebpEntropyPasses { get; set; }

    public int WebpSpatialNoiseShaping { get; set; }

    public int WebpFilterStrength { get; set; }

    public bool WebpSkipMetadata { get; set; } = true;

    /// <summary>Если true и вход — GIF, оригинал не перекодируется в WebP (анимация не ломается).</summary>
    public bool PreserveGifAsOriginal { get; set; } = true;

    /// <summary>
    /// Уровень CQ для AVIF (libaom), 0–63: меньше — выше качество и обычно больше файл. Игнорируется при <see cref="Format"/> не Avif.
    /// </summary>
    public int AvifCqLevel { get; set; } = 22;
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
