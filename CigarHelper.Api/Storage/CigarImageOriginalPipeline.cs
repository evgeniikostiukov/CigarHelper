using CigarHelper.Api.Helpers;
using CigarHelper.Api.Options;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Подготовка байтов оригинала перед записью в хранилище (перекодирование WebP, AVIF или «как загрузили»).
/// AVIF: пакет NeoSolve + бинарники avifenc в каталоге приложения (см. <c>ImageStorage:Compression:Original</c>).
/// </summary>
public static class CigarImageOriginalPipeline
{
    /// <summary>
    /// Возвращает байты для сохранения, MIME и имя файла для ключа (без GUID-префикса — его добавляет storage).
    /// </summary>
    public static async Task<(byte[] Blob, string SuggestedFileName, string ContentType)> PrepareOriginalAsync(
        byte[] uploadedBytes,
        string? originalFileName,
        string? declaredContentType,
        ImageStorageOptions options,
        CancellationToken ct = default)
    {
        var profile = options.Compression.Original;

        if (profile.Format.Equals("KeepOriginal", StringComparison.OrdinalIgnoreCase))
        {
            var name = string.IsNullOrWhiteSpace(originalFileName) ? "image" : originalFileName.Trim();
            var ctOut = !string.IsNullOrWhiteSpace(declaredContentType)
                ? declaredContentType!
                : ImageBinaryValidator.SuggestContentType(uploadedBytes) ?? "application/octet-stream";
            return (uploadedBytes, name, ctOut);
        }

        if (profile.PreserveGifAsOriginal && IsGif(uploadedBytes))
        {
            var nameGif = string.IsNullOrWhiteSpace(originalFileName) ? "image.gif" : originalFileName.Trim();
            var ctGif = !string.IsNullOrWhiteSpace(declaredContentType) && declaredContentType.Contains("gif", StringComparison.OrdinalIgnoreCase)
                ? declaredContentType!
                : "image/gif";
            return (uploadedBytes, nameGif, ctGif);
        }

        var stem = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(originalFileName) ? "image" : originalFileName.Trim());
        if (stem.Length == 0)
            stem = "image";

        if (profile.Format.Equals("Avif", StringComparison.OrdinalIgnoreCase))
        {
            var encoded = await ImageSharpRasterEncoder.EncodeAvifAsync(uploadedBytes, profile, ct);
            return (encoded, stem + ".avif", "image/avif");
        }

        var webp = await ImageSharpRasterEncoder.EncodeWebpAsync(uploadedBytes, profile, ct);
        return (webp, stem + ".webp", "image/webp");
    }

    private static bool IsGif(ReadOnlySpan<byte> d)
    {
        if (d.Length < 6) return false;
        return d[0] == (byte)'G'
               && d[1] == (byte)'I'
               && d[2] == (byte)'F'
               && d[3] == (byte)'8'
               && (d[4] == (byte)'7' || d[4] == (byte)'9')
               && d[5] == (byte)'a';
    }
}
