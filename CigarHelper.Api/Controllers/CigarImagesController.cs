using System.Security.Claims;
using CigarHelper.Api.Helpers;
using CigarHelper.Api.Options;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CigarImagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ImageUploadOptions _uploadOptions;
    private readonly IImageService _imageService;

    public CigarImagesController(
        AppDbContext context,
        IOptions<ImageUploadOptions> uploadOptions,
        IImageService imageService)
    {
        _context = context;
        _uploadOptions = uploadOptions.Value;
        _imageService = imageService;
    }

    private int GetCurrentUserId()
    {
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
        return int.Parse(s!);
    }

    private bool IsStaff() =>
        User.IsInRole(nameof(Role.Admin)) || User.IsInRole(nameof(Role.Moderator));

    private async Task<bool> UserOwnsUserCigarAsync(int userCigarId, int userId, CancellationToken ct) =>
        await _context.UserCigars.AnyAsync(uc => uc.Id == userCigarId && uc.UserId == userId, ct);

    /// <summary>Скачивает изображение по URL и сохраняет (только staff).</summary>
    [HttpPost("upload-by-url")]
    public async Task<ActionResult<object>> UploadCigarImageByUrl(
        [FromBody] UploadCigarImageByUrlRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsStaff())
            return Forbid();

        if (request is null)
            return BadRequest("Тело запроса пустое.");

        if (string.IsNullOrWhiteSpace(request.Url))
            return BadRequest("Укажите URL изображения.");

        if (request.CigarBaseId.HasValue &&
            !await _context.CigarBases.AnyAsync(cb => cb.Id == request.CigarBaseId.Value, cancellationToken))
            return NotFound("CigarBase не найдена.");

        var url = request.Url.Trim();
        var imageBytes = await ImageDownloader.DownloadImageAsync(url);
        if (imageBytes == null || imageBytes.Length == 0)
            return BadRequest("Не удалось загрузить изображение по указанной ссылке.");

        var contentType = GetContentTypeFromUrl(url);
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = ImageBinaryValidator.SuggestContentType(imageBytes);
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = "image/jpeg";

        if (!ImageBinaryValidator.TryValidate(
                imageBytes,
                contentType,
                imageBytes.Length,
                _uploadOptions.MaxBytes,
                out var validateError))
            return BadRequest(validateError);

        var cigarImage = await _imageService.SaveImageAsync(
            imageData: imageBytes,
            contentType: contentType,
            fileName: GetFileNameFromUrl(url),
            description: null,
            isMain: false,
            cigarBaseId: request.CigarBaseId,
            userCigarId: null,
            ct: cancellationToken);

        return Ok(new { id = cigarImage.Id });
    }

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

        // Не выбираем бинарные данные в списке — только метаданные
        var images = await query
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
                HasThumbnail = ci.ThumbnailData != null || ci.ThumbnailPath != null
            })
            .ToListAsync(cancellationToken);

        return Ok(images);
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

        return Ok(new CigarImageDto
        {
            Id = image.Id,
            FileName = image.FileName,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            Description = image.Description,
            IsMain = image.IsMain,
            CigarBaseId = image.CigarBaseId,
            UserCigarId = image.UserCigarId,
            CreatedAt = image.CreatedAt,
            HasThumbnail = image.ThumbnailData != null || image.ThumbnailPath != null
        });
    }

    /// <summary>Отдаёт бинарные данные полного изображения.</summary>
    [HttpGet("{id}/data")]
    public async Task<IActionResult> GetImageData(int id, CancellationToken cancellationToken)
    {
        var image = await _context.CigarImages.AsNoTracking().FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
        if (image == null) return NotFound();

        var userId = GetCurrentUserId();
        if (image.UserCigarId.HasValue && !IsStaff() &&
            !await UserOwnsUserCigarAsync(image.UserCigarId.Value, userId, cancellationToken))
            return NotFound();

        var (data, contentType) = await _imageService.GetImageDataAsync(image, cancellationToken);
        if (data == null || data.Length == 0)
            return NotFound("Бинарные данные изображения отсутствуют.");

        return File(data, contentType);
    }

    /// <summary>Отдаёт миниатюру изображения (WebP, 320×320 max).</summary>
    [HttpGet("{id}/thumbnail")]
    public async Task<IActionResult> GetThumbnail(int id, CancellationToken cancellationToken)
    {
        var image = await _context.CigarImages.AsNoTracking().FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
        if (image == null) return NotFound();

        var userId = GetCurrentUserId();
        if (image.UserCigarId.HasValue && !IsStaff() &&
            !await UserOwnsUserCigarAsync(image.UserCigarId.Value, userId, cancellationToken))
            return NotFound();

        var thumbData = await _imageService.GetThumbnailDataAsync(image, cancellationToken);
        if (thumbData == null || thumbData.Length == 0)
        {
            // Если миниатюры нет — возвращаем оригинал
            var (origData, origType) = await _imageService.GetImageDataAsync(image, cancellationToken);
            if (origData == null || origData.Length == 0)
                return NotFound("Данные изображения отсутствуют.");
            return File(origData, origType);
        }

        return File(thumbData, "image/webp");
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
            if (!staff) return Forbid();
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

        if (!ImageBinaryValidator.TryValidate(
                request.ImageData,
                request.ContentType,
                request.FileSize,
                _uploadOptions.MaxBytes,
                out var createError))
            return BadRequest(createError);

        var contentType = request.ContentType;
        if (request.ImageData is { Length: > 0 } data && string.IsNullOrWhiteSpace(contentType))
            contentType = ImageBinaryValidator.SuggestContentType(data);

        await ClearMainFlagIfNeededAsync(request.IsMain, request.CigarBaseId, request.UserCigarId, null, cancellationToken);

        var cigarImage = await _imageService.SaveImageAsync(
            imageData: request.ImageData,
            contentType: contentType,
            fileName: request.FileName,
            description: request.Description,
            isMain: request.IsMain,
            cigarBaseId: request.CigarBaseId,
            userCigarId: request.UserCigarId,
            ct: cancellationToken);

        return CreatedAtAction(nameof(GetCigarImage), new { id = cigarImage.Id }, ToDto(cigarImage));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCigarImage(
        int id,
        UpdateCigarImageRequest request,
        CancellationToken cancellationToken)
    {
        var cigarImage = await _context.CigarImages.FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
        if (cigarImage == null) return NotFound();

        var access = await ResolveImageAccessAsync(cigarImage, cancellationToken);
        if (!access.CanMutate) return access.FailResult!;

        if (request.ImageData != null)
        {
            var declaredType = request.ContentType ?? cigarImage.ContentType;
            if (!ImageBinaryValidator.TryValidate(
                    request.ImageData,
                    declaredType,
                    request.FileSize,
                    _uploadOptions.MaxBytes,
                    out var updateErr))
                return BadRequest(updateErr);

            var newType = request.ContentType
                ?? ImageBinaryValidator.SuggestContentType(request.ImageData)
                ?? cigarImage.ContentType;

            await _imageService.UpdateImageDataAsync(cigarImage, request.ImageData, newType, cancellationToken);
        }

        if (request.FileName != null) cigarImage.FileName = request.FileName;
        if (request.ContentType != null) cigarImage.ContentType = request.ContentType;
        if (request.FileSize.HasValue && request.ImageData == null) cigarImage.FileSize = request.FileSize;
        if (request.Description != null) cigarImage.Description = request.Description;

        if (request.IsMain.HasValue)
        {
            if (request.IsMain.Value && !cigarImage.IsMain)
            {
                await ClearMainFlagIfNeededAsync(true, cigarImage.CigarBaseId, cigarImage.UserCigarId, cigarImage.Id, cancellationToken);
            }
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
        if (cigarImage == null) return NotFound();

        var access = await ResolveImageAccessAsync(cigarImage, cancellationToken);
        if (!access.CanMutate) return access.FailResult!;

        await _imageService.DeleteImageAsync(cigarImage, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}/set-main")]
    public async Task<IActionResult> SetMainImage(int id, CancellationToken cancellationToken)
    {
        var cigarImage = await _context.CigarImages.FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
        if (cigarImage == null) return NotFound();

        var access = await ResolveImageAccessAsync(cigarImage, cancellationToken);
        if (!access.CanMutate) return access.FailResult!;

        await ClearMainFlagIfNeededAsync(true, cigarImage.CigarBaseId, cigarImage.UserCigarId, cigarImage.Id, cancellationToken);

        cigarImage.IsMain = true;
        cigarImage.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task ClearMainFlagIfNeededAsync(
        bool isMain,
        int? cigarBaseId,
        int? userCigarId,
        int? excludeId,
        CancellationToken ct)
    {
        if (!isMain) return;

        IQueryable<CigarImage> q = _context.CigarImages.Where(ci => ci.IsMain);
        if (excludeId.HasValue) q = q.Where(ci => ci.Id != excludeId.Value);

        if (cigarBaseId.HasValue)
        {
            var others = await q.Where(ci => ci.CigarBaseId == cigarBaseId.Value).ToListAsync(ct);
            foreach (var img in others) img.IsMain = false;
        }

        if (userCigarId.HasValue)
        {
            var others = await q.Where(ci => ci.UserCigarId == userCigarId.Value).ToListAsync(ct);
            foreach (var img in others) img.IsMain = false;
        }
    }

    private async Task<(bool CanMutate, IActionResult? FailResult)> ResolveImageAccessAsync(
        CigarImage image,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var staff = IsStaff();

        if (image.CigarBaseId.HasValue)
            return staff ? (true, null) : (false, Forbid());

        if (image.UserCigarId.HasValue)
        {
            if (staff || await UserOwnsUserCigarAsync(image.UserCigarId.Value, userId, ct))
                return (true, null);
            return (false, NotFound());
        }

        return (false, NotFound());
    }

    private static CigarImageDto ToDto(CigarImage image) => new()
    {
        Id = image.Id,
        FileName = image.FileName,
        ContentType = image.ContentType,
        FileSize = image.FileSize,
        Description = image.Description,
        IsMain = image.IsMain,
        CigarBaseId = image.CigarBaseId,
        UserCigarId = image.UserCigarId,
        CreatedAt = image.CreatedAt,
        HasThumbnail = image.ThumbnailData != null || image.ThumbnailPath != null
    };

    private static string GetFileNameFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var fileName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrEmpty(fileName)) return fileName;
        }
        return Guid.NewGuid().ToString("N")[..8] + ".jpg";
    }

    private static string GetContentTypeFromUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return "image/jpeg";

        return Path.GetExtension(uri.LocalPath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }
}
