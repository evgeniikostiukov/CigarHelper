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
                
                // Проверяем, что это действительно изображение
                if (IsValidImage(imageBytes))
                {
                    return imageBytes;
                }
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не прерываем выполнение
            Console.WriteLine($"Ошибка при загрузке изображения {imageUrl}: {ex.Message}");
        }
        
        return null;
    }
    
    private static bool IsValidImage(byte[] imageBytes)
    {
        if (imageBytes.Length == 0)
            return false;
            
        // Проверяем сигнатуры популярных форматов изображений
        var jpegSignature = new byte[] { 0xFF, 0xD8, 0xFF };
        var pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var gifSignature = new byte[] { 0x47, 0x49, 0x46 };
        var webpSignature = new byte[] { 0x52, 0x49, 0x46, 0x46 };
        
        return StartsWith(imageBytes, jpegSignature) ||
               StartsWith(imageBytes, pngSignature) ||
               StartsWith(imageBytes, gifSignature) ||
               StartsWith(imageBytes, webpSignature);
    }
    
    private static bool StartsWith(byte[] array, byte[] pattern)
    {
        if (array.Length < pattern.Length)
            return false;
            
        for (int i = 0; i < pattern.Length; i++)
        {
            if (array[i] != pattern[i])
                return false;
        }
        
        return true;
    }
} 