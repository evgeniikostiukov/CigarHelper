namespace CigarHelper.Api.Helpers;

/// <summary>Проверка magic numbers и согласованности с объявленным MIME для загрузок изображений.</summary>
public static class ImageBinaryValidator
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/avif"
    };

    public static bool IsRecognizedImage(ReadOnlySpan<byte> data) =>
        DetectFormat(data) != ImageFormat.Unknown;

    /// <summary>MIME по сигнатуре; null если формат неизвестен.</summary>
    public static string? SuggestContentType(ReadOnlySpan<byte> data) =>
        ContentTypeForFormat(DetectFormat(data));

    private static string? ContentTypeForFormat(ImageFormat format) =>
        format switch
        {
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Png => "image/png",
            ImageFormat.Gif => "image/gif",
            ImageFormat.Webp => "image/webp",
            ImageFormat.Avif => "image/avif",
            _ => null
        };

    /// <summary>
    /// Если <paramref name="imageData"/> пустой или null — успех (метаданные без файла).
    /// Иначе: лимит размера, известный формат, при наличии <paramref name="declaredContentType"/> — совпадение с сигнатурой.
    /// </summary>
    public static bool TryValidate(
        byte[]? imageData,
        string? declaredContentType,
        long? declaredFileSize,
        long maxBytes,
        out string? error)
    {
        error = null;
        if (imageData == null || imageData.Length == 0)
            return true;

        if (imageData.Length > maxBytes)
        {
            error = $"Размер изображения превышает лимит ({maxBytes} байт).";
            return false;
        }

        if (declaredFileSize.HasValue && declaredFileSize.Value != imageData.Length)
        {
            error = "Указанный FileSize не совпадает с размером данных изображения.";
            return false;
        }

        var format = DetectFormat(imageData);
        if (format == ImageFormat.Unknown)
        {
            error = "Файл не распознан как поддерживаемое изображение (JPEG, PNG, GIF, WebP, AVIF).";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(declaredContentType))
        {
            if (!AllowedContentTypes.Contains(declaredContentType))
            {
                error = "Недопустимый Content-Type для изображения.";
                return false;
            }

            if (!DeclaredTypeMatchesFormat(declaredContentType, format))
            {
                error = "Content-Type не соответствует содержимому файла.";
                return false;
            }
        }

        return true;
    }

    private static bool DeclaredTypeMatchesFormat(string declared, ImageFormat format)
    {
        var expected = ContentTypeForFormat(format);
        return expected != null
               && string.Equals(declared, expected, StringComparison.OrdinalIgnoreCase);
    }

    private enum ImageFormat
    {
        Unknown,
        Jpeg,
        Png,
        Gif,
        Webp,
        Avif
    }

    private static ImageFormat DetectFormat(ReadOnlySpan<byte> d)
    {
        if (d.Length >= 3 && d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF)
            return ImageFormat.Jpeg;

        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        if (d.Length >= png.Length && d[..png.Length].SequenceEqual(png))
            return ImageFormat.Png;

        if (d.Length >= 6
            && d[0] == (byte)'G'
            && d[1] == (byte)'I'
            && d[2] == (byte)'F'
            && d[3] == (byte)'8'
            && (d[4] == (byte)'7' || d[4] == (byte)'9')
            && d[5] == (byte)'a')
            return ImageFormat.Gif;

        // RIFF....WEBP
        if (d.Length >= 12
            && d[0] == (byte)'R'
            && d[1] == (byte)'I'
            && d[2] == (byte)'F'
            && d[3] == (byte)'F'
            && d[8] == (byte)'W'
            && d[9] == (byte)'E'
            && d[10] == (byte)'B'
            && d[11] == (byte)'P')
            return ImageFormat.Webp;

        // ISO BMFF: size(4) + "ftyp" + major brand "avif"
        if (d.Length >= 12
            && d[4] == (byte)'f'
            && d[5] == (byte)'t'
            && d[6] == (byte)'y'
            && d[7] == (byte)'p'
            && d[8] == (byte)'a'
            && d[9] == (byte)'v'
            && d[10] == (byte)'i'
            && d[11] == (byte)'f')
            return ImageFormat.Avif;

        return ImageFormat.Unknown;
    }
}
