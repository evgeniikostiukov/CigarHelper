using CigarHelper.Api.Helpers;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BrandsController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
    {
        var brands = await _context.Brands
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Country = b.Country,
                Description = b.Description,
                LogoBytes = b.LogoBytes,
                IsModerated = b.IsModerated,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .OrderBy(b => b.Name)
            .ToListAsync();
            
        return Ok(brands);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BrandDto>> GetBrand(int id)
    {
        var brand = await _context.Brands
            .Where(b => b.Id == id)
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Country = b.Country,
                Description = b.Description,
                LogoBytes = b.LogoBytes,
                IsModerated = b.IsModerated,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync();
            
        if (brand == null)
            return NotFound();
            
        return Ok(brand);
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult<BrandDto>> CreateBrand(CreateBrandRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        // Проверяем, не существует ли уже бренд с таким названием
        var existingBrand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Name.ToLower() == request.Name.ToLower());
            
        if (existingBrand != null)
        {
            return BadRequest($"Бренд с названием '{request.Name}' уже существует");
        }
        
        // Загружаем логотип, если указан URL
        byte[]? logoBytes = null;
        if (!string.IsNullOrWhiteSpace(request.LogoUrl))
        {
            logoBytes = await ImageDownloader.DownloadImageAsync(request.LogoUrl);
        }
        
        var brand = new Brand
        {
            Name = request.Name,
            Country = request.Country,
            Description = request.Description,
            LogoBytes = logoBytes,
            IsModerated = request.IsModerated,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Country = brand.Country,
            Description = brand.Description,
            LogoBytes = brand.LogoBytes,
            IsModerated = brand.IsModerated,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        });
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult<BrandDto>> UpdateBrand(int id, UpdateBrandRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var brand = await _context.Brands.FindAsync(id);
        if (brand == null)
            return NotFound();
        
        // Проверяем, не существует ли уже бренд с таким названием (кроме текущего)
        var existingBrand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Name.ToLower() == request.Name.ToLower() && b.Id != id);
            
        if (existingBrand != null)
        {
            return BadRequest($"Бренд с названием '{request.Name}' уже существует");
        }
        
        // Загружаем логотип, если URL есть
        if (!string.IsNullOrWhiteSpace(request.LogoUrl))
        {
            var logoBytes = await ImageDownloader.DownloadImageAsync(request.LogoUrl);
            if (logoBytes != null)
            {
                brand.LogoBytes = logoBytes;
            }
        }
        
        brand.Name = request.Name;
        brand.Country = request.Country;
        brand.Description = request.Description;
        brand.IsModerated = request.IsModerated;
        brand.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return Ok(new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Country = brand.Country,
            Description = brand.Description,
            LogoBytes = brand.LogoBytes,
            IsModerated = brand.IsModerated,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        });
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand == null)
            return NotFound();
        
        // Проверяем, не используется ли бренд в сигарах
        var hasCigars = await _context.CigarBases.AnyAsync(cb => cb.BrandId == id);
        if (hasCigars)
        {
            return BadRequest("Нельзя удалить бренд, который используется в сигарах");
        }
        
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
} 