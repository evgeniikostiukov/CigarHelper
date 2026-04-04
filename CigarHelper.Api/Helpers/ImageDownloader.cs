using System.Net.Http;
using System.Threading.Tasks;

namespace CigarHelper.Api.Helpers;

public static class ImageDownloader
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    public static async Task<byte[]?> DownloadImageAsync(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        var fromDataUri = TryDecodeDataImageUri(imageUrl);
        if (fromDataUri != null)
            return fromDataUri;

        try
        {
            // Устанавливаем User-Agent для избежания блокировки некоторыми сайтами
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            
            var response = await _httpClient.GetAsync(imageUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                
                if (ImageBinaryValidator.IsRecognizedImage(imageBytes))
                {
                    return imageBytes;
                }
            }
        }
        catch
        {
            // Не раскрываем URL/детали наружу; вызывающий код получает null.
        }

        return null;
    }

    /// <summary>Декодирует data:image/...;base64,... в байты после проверки сигнатуры (локальные файлы с формы).</summary>
    private static byte[]? TryDecodeDataImageUri(string imageUrl)
    {
        if (!imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return null;

        const string base64Marker = ";base64,";
        var idx = imageUrl.IndexOf(base64Marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return null;

        try
        {
            var base64 = imageUrl[(idx + base64Marker.Length)..].Trim();
            var bytes = Convert.FromBase64String(base64);
            return ImageBinaryValidator.IsRecognizedImage(bytes) ? bytes : null;
        }
        catch
        {
            return null;
        }
    }
} 