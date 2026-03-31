using CigarHelper.Data.Data;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Глобальный поиск по сигарам пользователя, хьюмидорам, базе сигар и брендам.
    /// Минимальная длина запроса — 2 символа; результаты ограничены параметром limit (default 5, max 10).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GlobalSearchResultDto>> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Ok(new GlobalSearchResultDto());

        if (limit < 1 || limit > 10) limit = 5;

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var term = q.Trim().ToLower();

        var cigarsTask = _context.UserCigars
            .Include(uc => uc.CigarBase).ThenInclude(cb => cb.Brand)
            .Include(uc => uc.Humidor)
            .Where(uc => uc.UserId == userId &&
                         (uc.CigarBase.Name.ToLower().Contains(term) ||
                          uc.CigarBase.Brand.Name.ToLower().Contains(term)))
            .OrderBy(uc => uc.CigarBase.Name)
            .Take(limit)
            .Select(uc => new SearchCigarDto
            {
                Id = uc.Id,
                Name = uc.CigarBase.Name,
                BrandName = uc.CigarBase.Brand.Name,
                HumidorName = uc.Humidor != null ? uc.Humidor.Name : null
            })
            .ToListAsync();

        var humidorsTask = _context.Humidors
            .Where(h => h.UserId == userId && h.Name.ToLower().Contains(term))
            .OrderBy(h => h.Name)
            .Take(limit)
            .Select(h => new SearchHumidorDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description
            })
            .ToListAsync();

        var cigarBasesTask = _context.CigarBases
            .Include(cb => cb.Brand)
            .Where(cb => cb.IsModerated &&
                         (cb.Name.ToLower().Contains(term) ||
                          cb.Brand.Name.ToLower().Contains(term)))
            .OrderBy(cb => cb.Name)
            .Take(limit)
            .Select(cb => new SearchCigarBaseDto
            {
                Id = cb.Id,
                Name = cb.Name,
                BrandName = cb.Brand.Name
            })
            .ToListAsync();

        var brandsTask = _context.Brands
            .Where(b => b.IsModerated && b.Name.ToLower().Contains(term))
            .OrderBy(b => b.Name)
            .Take(limit)
            .Select(b => new SearchBrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Country = b.Country
            })
            .ToListAsync();

        await Task.WhenAll(cigarsTask, humidorsTask, cigarBasesTask, brandsTask);

        return Ok(new GlobalSearchResultDto
        {
            Cigars = await cigarsTask,
            Humidors = await humidorsTask,
            CigarBases = await cigarBasesTask,
            Brands = await brandsTask
        });
    }
}
