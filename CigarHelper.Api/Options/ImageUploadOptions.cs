namespace CigarHelper.Api.Options;

public class ImageUploadOptions
{
    public const string SectionName = "ImageUpload";

    /// <summary>Максимальный размер тела изображения в байтах (по умолчанию 5 MiB).</summary>
    public int MaxBytes { get; set; } = 5 * 1024 * 1024;
}
