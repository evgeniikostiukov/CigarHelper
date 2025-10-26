using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CigarImagesController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public CigarImagesController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CigarImageDto>>> GetCigarImages([FromQuery] int? cigarBaseId, [FromQuery] int? userCigarId)
    {
        IQueryable<CigarImage> query = _context.CigarImages;
        
        if (cigarBaseId.HasValue)
        {
            query = query.Where(ci => ci.CigarBaseId == cigarBaseId);
        }
        
        if (userCigarId.HasValue)
        {
            query = query.Where(ci => ci.UserCigarId == userCigarId);
        }
        
        var images = await query.ToListAsync();
        
        return Ok(images.Select(image => new CigarImageDto
        {
            Id = image.Id,
            FileName = image.FileName,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            Description = image.Description,
            IsMain = image.IsMain,
            CigarBaseId = image.CigarBaseId,
            UserCigarId = image.UserCigarId,
            CreatedAt = image.CreatedAt
        }));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<CigarImageDto>> GetCigarImage(int id)
    {
        var image = await _context.CigarImages.FindAsync(id);
        
        if (image == null)
        {
            return NotFound();
        }
        
        var imageDto = new CigarImageDto
        {
            Id = image.Id,
            FileName = image.FileName,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            Description = image.Description,
            IsMain = image.IsMain,
            CigarBaseId = image.CigarBaseId,
            UserCigarId = image.UserCigarId,
            CreatedAt = image.CreatedAt
        };
        
        return Ok(imageDto);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CigarImageDto>> CreateCigarImage(CreateCigarImageRequest request)
    {
        // Проверяем, что хотя бы один ID указан
        if (!request.CigarBaseId.HasValue && !request.UserCigarId.HasValue)
        {
            return BadRequest("Необходимо указать CigarBaseId или UserCigarId");
        }
        
        // Если указан CigarBaseId, проверяем его существование
        if (request.CigarBaseId.HasValue)
        {
            var cigarBase = await _context.CigarBases.FindAsync(request.CigarBaseId.Value);
            if (cigarBase == null)
            {
                return NotFound($"CigarBase с ID {request.CigarBaseId} не найден");
            }
        }
        
        // Если указан UserCigarId, проверяем его существование
        if (request.UserCigarId.HasValue)
        {
            var userCigar = await _context.UserCigars.FindAsync(request.UserCigarId.Value);
            if (userCigar == null)
            {
                return NotFound($"UserCigar с ID {request.UserCigarId} не найден");
            }
        }
        
        // Если это главное изображение, отменяем флаг IsMain у других изображений
        if (request.IsMain)
        {
            if (request.CigarBaseId.HasValue)
            {
                var existingMainImages = await _context.CigarImages
                    .Where(ci => ci.CigarBaseId == request.CigarBaseId.Value && ci.IsMain)
                    .ToListAsync();
                
                foreach (var img in existingMainImages)
                {
                    img.IsMain = false;
                }
            }
            
            if (request.UserCigarId.HasValue)
            {
                var existingMainImages = await _context.CigarImages
                    .Where(ci => ci.UserCigarId == request.UserCigarId.Value && ci.IsMain)
                    .ToListAsync();
                
                foreach (var img in existingMainImages)
                {
                    img.IsMain = false;
                }
            }
        }
        
        // Создаем новое изображение
        var cigarImage = new CigarImage
        {
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            Description = request.Description,
            IsMain = request.IsMain,
            CigarBaseId = request.CigarBaseId,
            UserCigarId = request.UserCigarId
        };
        
        _context.CigarImages.Add(cigarImage);
        await _context.SaveChangesAsync();
        
        var imageDto = new CigarImageDto
        {
            Id = cigarImage.Id,
            FileName = cigarImage.FileName,
            ContentType = cigarImage.ContentType,
            FileSize = cigarImage.FileSize,
            Description = cigarImage.Description,
            IsMain = cigarImage.IsMain,
            CigarBaseId = cigarImage.CigarBaseId,
            UserCigarId = cigarImage.UserCigarId,
            CreatedAt = cigarImage.CreatedAt
        };
        
        return CreatedAtAction(nameof(GetCigarImage), new { id = imageDto.Id }, imageDto);
    }
    
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateCigarImage(int id, UpdateCigarImageRequest request)
    {
        var cigarImage = await _context.CigarImages.FindAsync(id);
        
        if (cigarImage == null)
        {
            return NotFound();
        }
        
        // Обновляем поля
        if (request.FileName != null)
            cigarImage.FileName = request.FileName;
        
        if (request.ContentType != null)
            cigarImage.ContentType = request.ContentType;
        
        if (request.FileSize.HasValue)
            cigarImage.FileSize = request.FileSize;
        
        if (request.Description != null)
            cigarImage.Description = request.Description;
        
        if (request.IsMain.HasValue && request.IsMain.Value && !cigarImage.IsMain)
        {
            // Если изображение становится главным, отменяем этот флаг у других изображений
            if (cigarImage.CigarBaseId.HasValue)
            {
                var existingMainImages = await _context.CigarImages
                    .Where(ci => ci.CigarBaseId == cigarImage.CigarBaseId.Value && ci.IsMain && ci.Id != cigarImage.Id)
                    .ToListAsync();
                
                foreach (var img in existingMainImages)
                {
                    img.IsMain = false;
                }
            }
            
            if (cigarImage.UserCigarId.HasValue)
            {
                var existingMainImages = await _context.CigarImages
                    .Where(ci => ci.UserCigarId == cigarImage.UserCigarId.Value && ci.IsMain && ci.Id != cigarImage.Id)
                    .ToListAsync();
                
                foreach (var img in existingMainImages)
                {
                    img.IsMain = false;
                }
            }
            
            cigarImage.IsMain = true;
        }
        else if (request.IsMain.HasValue)
        {
            cigarImage.IsMain = request.IsMain.Value;
        }
        
        cigarImage.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCigarImage(int id)
    {
        var cigarImage = await _context.CigarImages.FindAsync(id);
        
        if (cigarImage == null)
        {
            return NotFound();
        }
        
        _context.CigarImages.Remove(cigarImage);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    [HttpPatch("{id}/set-main")]
    [Authorize]
    public async Task<IActionResult> SetMainImage(int id)
    {
        var cigarImage = await _context.CigarImages.FindAsync(id);
        
        if (cigarImage == null)
        {
            return NotFound();
        }
        
        // Сбрасываем флаг IsMain у всех других изображений
        if (cigarImage.CigarBaseId.HasValue)
        {
            var existingMainImages = await _context.CigarImages
                .Where(ci => ci.CigarBaseId == cigarImage.CigarBaseId.Value && ci.IsMain && ci.Id != cigarImage.Id)
                .ToListAsync();
            
            foreach (var img in existingMainImages)
            {
                img.IsMain = false;
            }
        }
        
        if (cigarImage.UserCigarId.HasValue)
        {
            var existingMainImages = await _context.CigarImages
                .Where(ci => ci.UserCigarId == cigarImage.UserCigarId.Value && ci.IsMain && ci.Id != cigarImage.Id)
                .ToListAsync();
            
            foreach (var img in existingMainImages)
            {
                img.IsMain = false;
            }
        }
        
        cigarImage.IsMain = true;
        cigarImage.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
} 