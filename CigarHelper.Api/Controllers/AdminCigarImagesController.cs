using CigarHelper.Api.Models;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/admin/cigar-images")]
[Authorize(Roles = "Admin")]
public class AdminCigarImagesController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;

    public AdminCigarImagesController(AppDbContext context) => _context = context;

    /// <summary>Все загруженные изображения каталога и коллекций (таблица CigarImages, внешнее хранилище).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedCigarImagesAdminResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedCigarImagesAdminResponse>> GetImages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _context.CigarImages.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(ci => ci.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ci => new CigarImageDto
            {
                Id = ci.Id,
                FileName = ci.FileName,
                ContentType = ci.ContentType,
                FileSize = ci.FileSize,
                Description = ci.Description,
                IsMain = ci.IsMain,
                CigarBaseId = ci.CigarBaseId,
                UserCigarId = ci.UserCigarId,
                CreatedAt = ci.CreatedAt,
                HasThumbnail = ci.ThumbnailPath != null
            })
            .ToListAsync(cancellationToken);

        return Ok(new PagedCigarImagesAdminResponse
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }
}
