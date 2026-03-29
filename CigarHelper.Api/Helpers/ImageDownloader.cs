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
} 