using System.Security.Claims;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CigarImagesController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public CigarImagesController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
        return int.Parse(s!);
    }

    private bool IsStaff() =>
        User.IsInRole(nameof(Role.Admin)) || User.IsInRole(nameof(Role.Moderator));

    private async Task<bool> UserOwnsUserCigarAsync(int userCigarId, int userId, CancellationToken cancellationToken) =>
        await _context.UserCigars.AnyAsync(uc => uc.Id == userCigarId && uc.UserId == userId, cancellationToken);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CigarImageDto>>> GetCigarImages(
        [FromQuery] int? cigarBaseId,
        [FromQuery] int? userCigarId,
        CancellationToken cancellationToken)
    {
        if (!cigarBaseId.HasValue && !userCigarId.HasValue)
            return BadRequest("Укажите cigarBaseId и/или userCigarId.");

        var userId = GetCurrentUserId();
        var staff = IsStaff();

        if (userCigarId.HasValue && !staff &&
            !await UserOwnsUserCigarAsync(userCigarId.Value, userId, cancellationToken))
            return NotFound();

        IQueryable<CigarImage> query = _context.CigarImages;

        if (cigarBaseId.HasValue)
            query = query.Where(ci => ci.CigarBaseId == cigarBaseId);

        if (userCigarId.HasValue)
            query = query.Where(ci => ci.UserCigarId == userCigarId);

        var images = await query.ToListAsync(cancellationToken);
        
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
    public async Task<ActionResult<CigarImageDto>> GetCigarImage(int id, CancellationToken cancellationToken)
    {
        var image = await _context.CigarImages.AsNoTracking().FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

        if (image == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var staff = IsStaff();
        if (image.UserCigarId.HasValue && !staff &&
            !await UserOwnsUserCigarAsync(image.UserCigarId.Value, userId, cancellationToken))
            return NotFound();

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
    public async Task<ActionResult<CigarImageDto>> CreateCigarImage(
        CreateCigarImageRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.CigarBaseId.HasValue && !request.UserCigarId.HasValue)
            return BadRequest("Необходимо указать CigarBaseId или UserCigarId.");

        if (request.CigarBaseId.HasValue && request.UserCigarId.HasValue)
            return BadRequest("Укажите только CigarBaseId или только UserCigarId.");

        var userId = GetCurrentUserId();
        var staff = IsStaff();

        if (request.CigarBaseId.HasValue)
        {
            if (!staff)
                return Forbid();

            var cigarBase = await _context.CigarBases.FindAsync(new object[] { request.CigarBaseId.Value }, cancellationToken);
            if (cigarBase == null)
                return NotFound($"CigarBase с ID {request.CigarBaseId} не найден");
        }

        if (request.UserCigarId.HasValue)
        {
            var userCigar = await _context.UserCigars.FindAsync(new object[] { request.UserCigarId.Value }, cancellationToken);
            if (userCigar == null)
                return NotFound($"UserCigar с ID {request.UserCigarId} не найден");

            if (!staff && userCigar.UserId != userId)
                return NotFound();
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
        await _context.SaveChangesAsync(cancellationToken);
        
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
    public async Task<IActionResult> UpdateCigarImage(
        int id,
        UpdateCigarImageRequest request,
        CancellationToken cancellationToken)
    {
        var cigarImage = await _context.CigarImages.FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

        if (cigarImage == null)
            return NotFound();

        var access = await ResolveImageAccessAsync(cigarImage, cancellationToken);
        if (!access.CanMutate)
            return access.FailResult!;

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

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCigarImage(int id, CancellationToken cancellationToken)
    {
        var cigarImage = await _context.CigarImages.FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

        if (cigarImage == null)
            return NotFound();

        var access = await ResolveImageAccessAsync(cigarImage, cancellationToken);
        if (!access.CanMutate)
            return access.FailResult!;

        _context.CigarImages.Remove(cigarImage);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}/set-main")]
    public async Task<IActionResult> SetMainImage(int id, CancellationToken cancellationToken)
    {
        var cigarImage = await _context.CigarImages.FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);

        if (cigarImage == null)
            return NotFound();

        var access = await ResolveImageAccessAsync(cigarImage, cancellationToken);
        if (!access.CanMutate)
            return access.FailResult!;

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

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<(bool CanMutate, IActionResult? FailResult)> ResolveImageAccessAsync(
        CigarImage image,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var staff = IsStaff();

        if (image.CigarBaseId.HasValue)
        {
            if (!staff)
                return (false, Forbid());
            return (true, null);
        }

        if (image.UserCigarId.HasValue)
        {
            if (staff || await UserOwnsUserCigarAsync(image.UserCigarId.Value, userId, cancellationToken))
                return (true, null);
            return (false, NotFound());
        }

        return (false, NotFound());
    }
}