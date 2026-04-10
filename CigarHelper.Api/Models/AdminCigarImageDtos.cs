using CigarHelper.Data.Models.Dtos;

namespace CigarHelper.Api.Models;

/// <summary>
/// Пагинированный список изображений сигар (хранилище MinIO/LocalFile) для админ-панели.
/// </summary>
public class PagedCigarImagesAdminResponse
{
    public IReadOnlyList<CigarImageDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
