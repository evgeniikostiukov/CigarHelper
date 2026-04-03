using CigarHelper.Api.Helpers;
using CigarHelper.Api.Models;
using CigarHelper.Api.Services;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CigarsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;

    public CigarsController(AppDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CigarResponseDto>>> GetCigars()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigars = await _context.UserCigars
            .Include(uc => uc.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Include(uc => uc.Humidor)
            .Where(uc => uc.UserId == userId)
            .Select(uc => new CigarResponseDto
            {
                Id = uc.Id,
                Name = uc.CigarBase.Name,
                Brand = new BrandDto()
                {
                    Id = uc.CigarBase.Brand.Id,
                    Name = uc.CigarBase.Brand.Name,
                    Description = uc.CigarBase.Brand.Description,
                    UpdatedAt = uc.CigarBase.Brand.UpdatedAt,
                    CreatedAt = uc.CigarBase.Brand.CreatedAt,
                    Country = uc.CigarBase.Brand.Country,
                    IsModerated = uc.CigarBase.Brand.IsModerated,
                    LogoBytes = uc.CigarBase.Brand.LogoBytes,
                },
                BrandName = uc.CigarBase.Brand.Name,
                Size = uc.CigarBase.Size,
                Strength = uc.CigarBase.Strength,
                Price = uc.Price,
                Rating = uc.Rating,
                Country = uc.CigarBase.Country,
                Description = uc.CigarBase.Description,
                Wrapper = uc.CigarBase.Wrapper,
                Binder = uc.CigarBase.Binder,
                Filler = uc.CigarBase.Filler,
                HumidorId = uc.HumidorId,
                Humidor = uc.Humidor != null ? new HumidorDto
                {
                    Id = uc.Humidor.Id,
                    Name = uc.Humidor.Name,
                    Description = uc.Humidor.Description,
                    Capacity = uc.Humidor.Capacity,
                    CreatedAt = uc.Humidor.CreatedAt,
                    UpdatedAt = uc.Humidor.UpdatedAt
                } : null,
                Images = _context.CigarImages
                    .Where(img => img.UserCigarId == uc.Id && (img.ImageData != null || img.StoragePath != null))
                    .OrderByDescending(img => img.IsMain)
                    .ThenBy(img => img.Id)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                UserId = uc.UserId,
                CreatedAt = uc.CreatedAt,
                UpdatedAt = uc.UpdatedAt,
                PurchasedAt = uc.PurchasedAt,
                SmokedAt = uc.SmokedAt,
                LastTouchedAt = uc.LastTouchedAt
            })
            .ToListAsync();

        return Ok(cigars);
    }

    [HttpGet("bases")]
    public async Task<ActionResult<IEnumerable<CigarBaseDto>>> GetAllCigarBases([FromQuery] string? search = null)
    {
        var cigarBasesQuery = _context.CigarBases
            .Include(cb => cb.Brand)
            .Include(cb => cb.Images)
            .Where(cb => cb.IsModerated); // Только проверенные сигары

        // Применяем фильтры
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            cigarBasesQuery = cigarBasesQuery.Where(cb =>
                cb.Name.ToLower().Contains(searchLower) ||
                cb.Brand.Name.ToLower().Contains(searchLower));
        }

        var cigarBases = await cigarBasesQuery
            .Select(cb => new CigarBaseDto
            {
                Id = cb.Id,
                Name = cb.Name,
                Brand = new BrandDto
                {
                    Id = cb.BrandId,
                    Name = cb.Brand.Name,
                    Country = cb.Brand.Country,
                    Description = cb.Brand.Description,
                    LogoBytes = cb.Brand.LogoBytes,
                    IsModerated = cb.Brand.IsModerated,
                    CreatedAt = cb.Brand.CreatedAt,
                    UpdatedAt = cb.Brand.UpdatedAt
                },
                Size = cb.Size,
                Strength = cb.Strength,
                Country = cb.Country,
                Description = cb.Description,
                Wrapper = cb.Wrapper,
                Binder = cb.Binder,
                Filler = cb.Filler,
                Images = cb.Images.Where(img => img.ImageData != null || img.StoragePath != null)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                CreatedAt = cb.CreatedAt,
                UpdatedAt = cb.UpdatedAt
            })
            .OrderBy(cb => cb.Name)
            .ToListAsync();

        return Ok(cigarBases);
    }

    [HttpGet("bases/paginated")]
    public async Task<ActionResult<PaginatedResult<CigarBaseDto>>> GetCigarBasesPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortField = "name",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] string? search = null,
        [FromQuery] int? brandId = null,
        [FromQuery] string? strength = null,
        [FromQuery] bool excludeBinaryMedia = false)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.CigarBases
            .Where(cb => cb.IsModerated); // Только проверенные сигары

        // Применяем фильтры
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(cb =>
                cb.Name.ToLower().Contains(searchLower) ||
                cb.Brand.Name.ToLower().Contains(searchLower));
        }

        if (brandId.HasValue)
        {
            query = query.Where(cb => cb.BrandId == brandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(strength))
        {
            query = query.Where(cb => cb.Strength == strength);
        }

        // Получаем общее количество записей
        var totalCount = await query.CountAsync();

        // Применяем сортировку
        query = sortField?.ToLower() switch
        {
            "name" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(cb => cb.Name) : query.OrderBy(cb => cb.Name),
            "brandname" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(cb => cb.Brand.Name) : query.OrderBy(cb => cb.Brand.Name),
            "size" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(cb => cb.Size) : query.OrderBy(cb => cb.Size),
            "strength" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(cb => cb.Strength) : query.OrderBy(cb => cb.Strength),
            "country" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(cb => cb.Country) : query.OrderBy(cb => cb.Country),
            _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(cb => cb.Name) : query.OrderBy(cb => cb.Name)
        };

        // Пагинация: без excludeBinaryMedia тянем bytea (лого, фото) — при pageSize=100 ответ раздувается и часто падает по памяти/таймауту.
        var paged = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        List<CigarBaseDto> items;
        if (excludeBinaryMedia)
        {
            items = await paged
                .Select(cb => new CigarBaseDto
                {
                    Id = cb.Id,
                    Name = cb.Name,
                    Brand = new BrandDto
                    {
                        Id = cb.BrandId,
                        Name = cb.Brand.Name,
                        Country = cb.Brand.Country,
                        Description = cb.Brand.Description,
                        LogoBytes = null,
                        IsModerated = cb.Brand.IsModerated,
                        CreatedAt = cb.Brand.CreatedAt,
                        UpdatedAt = cb.Brand.UpdatedAt
                    },
                    Size = cb.Size,
                    Strength = cb.Strength,
                    Country = cb.Country,
                    Description = cb.Description,
                    Wrapper = cb.Wrapper,
                    Binder = cb.Binder,
                    Filler = cb.Filler,
                    Images = cb.Images
                        .Select(img => new CigarImageDto
                        {
                            Id = img.Id,
                            FileName = img.FileName,
                            ContentType = img.ContentType,
                            FileSize = img.FileSize,
                            Description = img.Description,
                            IsMain = img.IsMain,
                            CigarBaseId = img.CigarBaseId,
                            UserCigarId = img.UserCigarId,
                            CreatedAt = img.CreatedAt,
                            HasThumbnail = false
                        }).ToList(),
                    CreatedAt = cb.CreatedAt,
                    UpdatedAt = cb.UpdatedAt
                })
                .ToListAsync();
        }
        else
        {
            items = await paged
                .Select(cb => new CigarBaseDto
                {
                    Id = cb.Id,
                    Name = cb.Name,
                    Brand = new BrandDto
                    {
                        Id = cb.BrandId,
                        Name = cb.Brand.Name,
                        Country = cb.Brand.Country,
                        Description = cb.Brand.Description,
                        LogoBytes = cb.Brand.LogoBytes,
                        IsModerated = cb.Brand.IsModerated,
                        CreatedAt = cb.Brand.CreatedAt,
                        UpdatedAt = cb.Brand.UpdatedAt
                    },
                    Size = cb.Size,
                    Strength = cb.Strength,
                    Country = cb.Country,
                    Description = cb.Description,
                    Wrapper = cb.Wrapper,
                    Binder = cb.Binder,
                    Filler = cb.Filler,
                    Images = cb.Images.Where(img => img.ImageData != null || img.StoragePath != null)
                        .Select(img => new CigarImageDto
                        {
                            Id = img.Id,
                            FileName = img.FileName,
                            ContentType = img.ContentType,
                            FileSize = img.FileSize,
                            Description = img.Description,
                            IsMain = img.IsMain,
                            CigarBaseId = img.CigarBaseId,
                            UserCigarId = img.UserCigarId,
                            CreatedAt = img.CreatedAt,
                            HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                        }).ToList(),
                    CreatedAt = cb.CreatedAt,
                    UpdatedAt = cb.UpdatedAt
                })
                .ToListAsync();
        }

        var result = new PaginatedResult<CigarBaseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CigarResponseDto>> GetCigar(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigar = await _context.UserCigars
            .Include(uc => uc.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Include(uc => uc.Humidor)
            .Where(uc => uc.Id == id && uc.UserId == userId)
            .Select(uc => new CigarResponseDto
            {
                Id = uc.Id,
                Name = uc.CigarBase.Name,
                Brand = new BrandDto()
                {
                    Id = uc.CigarBase.Brand.Id,
                    Name = uc.CigarBase.Brand.Name,
                    Description = uc.CigarBase.Brand.Description,
                    UpdatedAt = uc.CigarBase.Brand.UpdatedAt,
                    CreatedAt = uc.CigarBase.Brand.CreatedAt,
                    Country = uc.CigarBase.Brand.Country,
                    IsModerated = uc.CigarBase.Brand.IsModerated,
                    LogoBytes = uc.CigarBase.Brand.LogoBytes,
                },
                BrandName = uc.CigarBase.Brand.Name,
                Size = uc.CigarBase.Size,
                Strength = uc.CigarBase.Strength,
                Price = uc.Price,
                Rating = uc.Rating,
                Country = uc.CigarBase.Country,
                Description = uc.CigarBase.Description,
                Wrapper = uc.CigarBase.Wrapper,
                Binder = uc.CigarBase.Binder,
                Filler = uc.CigarBase.Filler,
                HumidorId = uc.HumidorId,
                Humidor = uc.Humidor != null ? new HumidorDto
                {
                    Id = uc.Humidor.Id,
                    Name = uc.Humidor.Name,
                    Description = uc.Humidor.Description,
                    Capacity = uc.Humidor.Capacity,
                    CreatedAt = uc.Humidor.CreatedAt,
                    UpdatedAt = uc.Humidor.UpdatedAt
                } : null,
                Images = _context.CigarImages
                    .Where(img => img.UserCigarId == uc.Id && (img.ImageData != null || img.StoragePath != null))
                    .OrderByDescending(img => img.IsMain)
                    .ThenBy(img => img.Id)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                UserId = uc.UserId,
                CreatedAt = uc.CreatedAt,
                UpdatedAt = uc.UpdatedAt,
                PurchasedAt = uc.PurchasedAt,
                SmokedAt = uc.SmokedAt,
                LastTouchedAt = uc.LastTouchedAt
            })
            .FirstOrDefaultAsync();

        if (cigar == null)
            return NotFound();

        return Ok(cigar);
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
    {
        var brands = await _context.Brands
            .Where(b => b.IsModerated)
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Country = b.Country,
                Description = b.Description,
                IsModerated = b.IsModerated,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .OrderBy(b => b.Name)
            .ToListAsync();

        return Ok(brands);
    }

    [HttpPost]
    public async Task<ActionResult<CigarResponseDto>> CreateCigar(CreateCigarRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Проверяем существование бренда
        var brand = await _context.Brands.FindAsync(request.BrandId);
        if (brand == null)
        {
            return BadRequest($"Бренд с ID {request.BrandId} не найден");
        }

        // Ищем существующую сигару в базе по названию и ID бренда
        var existingCigarBase = await _context.CigarBases
            .FirstOrDefaultAsync(cb => cb.Name.ToLower() == request.Name.ToLower() &&
                                     cb.BrandId == request.BrandId);

        int cigarBaseId;

        if (existingCigarBase != null)
        {
            // Используем существующую сигару из базы
            cigarBaseId = existingCigarBase.Id;
        }
        else
        {
            // Создаем новую сигару в базе с пометкой о том, что она не прошла модерацию
            var newCigarBase = new CigarBase
            {
                Name = request.Name,
                BrandId = request.BrandId,
                Country = request.Country,
                Description = request.Description,
                Strength = request.Strength,
                Size = request.Size,
                Wrapper = request.Wrapper,
                Binder = request.Binder,
                Filler = request.Filler,
                IsModerated = false, // Не прошла модерацию
                CreatedAt = DateTime.UtcNow
            };

            _context.CigarBases.Add(newCigarBase);
            await _context.SaveChangesAsync();

            cigarBaseId = newCigarBase.Id;
        }

        // Создаем сигару пользователя
        var userCigar = new UserCigar
        {
            CigarBaseId = cigarBaseId,
            Price = request.Price,
            Rating = request.Rating,
            HumidorId = request.HumidorId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            PurchasedAt = DateTime.UtcNow,
            LastTouchedAt = DateTime.UtcNow
        };

        _context.UserCigars.Add(userCigar);
        await _context.SaveChangesAsync();

        if (request.ImageUrls is { Count: > 0 })
            await TryAddUserCigarImagesFromOrderedUrlsAsync(userCigar.Id, request.ImageUrls);
        else
            await TryAddUserCigarImageFromUrlAsync(userCigar.Id, request.ImageUrl);

        // Возвращаем созданную сигару пользователя с данными из базы
        var createdCigar = await _context.UserCigars
            .Include(uc => uc.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Include(uc => uc.Humidor)
            .Where(uc => uc.Id == userCigar.Id)
            .Select(uc => new CigarResponseDto
            {
                Id = uc.Id,
                Name = uc.CigarBase.Name,
                Brand = new BrandDto()
                {
                    Id = uc.CigarBase.Brand.Id,
                    Name = uc.CigarBase.Brand.Name,
                    Description = uc.CigarBase.Brand.Description,
                    UpdatedAt = uc.CigarBase.Brand.UpdatedAt,
                    CreatedAt = uc.CigarBase.Brand.CreatedAt,
                    Country = uc.CigarBase.Brand.Country,
                    IsModerated = uc.CigarBase.Brand.IsModerated,
                    LogoBytes = uc.CigarBase.Brand.LogoBytes,
                },
                BrandName = uc.CigarBase.Brand.Name,
                Size = uc.CigarBase.Size,
                Strength = uc.CigarBase.Strength,
                Price = uc.Price,
                Rating = uc.Rating,
                Country = uc.CigarBase.Country,
                Description = uc.CigarBase.Description,
                Wrapper = uc.CigarBase.Wrapper,
                Binder = uc.CigarBase.Binder,
                Filler = uc.CigarBase.Filler,
                HumidorId = uc.HumidorId,
                Humidor = uc.Humidor != null ? new HumidorDto
                {
                    Id = uc.Humidor.Id,
                    Name = uc.Humidor.Name,
                    Description = uc.Humidor.Description,
                    Capacity = uc.Humidor.Capacity,
                    CreatedAt = uc.Humidor.CreatedAt,
                    UpdatedAt = uc.Humidor.UpdatedAt
                } : null,
                Images = _context.CigarImages
                    .Where(img => img.UserCigarId == uc.Id && (img.ImageData != null || img.StoragePath != null))
                    .OrderByDescending(img => img.IsMain)
                    .ThenBy(img => img.Id)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                UserId = uc.UserId,
                CreatedAt = uc.CreatedAt,
                UpdatedAt = uc.UpdatedAt,
                PurchasedAt = uc.PurchasedAt,
                SmokedAt = uc.SmokedAt,
                LastTouchedAt = uc.LastTouchedAt
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetCigar), new { id = userCigar.Id }, createdCigar);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CigarResponseDto>> UpdateCigar(int id, UserCigarUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var existingUserCigar = await _context.UserCigars
            .Include(uc => uc.CigarBase)
            .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == userId);

        if (existingUserCigar == null)
            return NotFound();

        // Проверяем существование бренда
        var brand = await _context.Brands.FindAsync(request.BrandId);
        if (brand == null)
        {
            return BadRequest($"Бренд с ID {request.BrandId} не найден");
        }

        // Обновляем поля пользовательской сигары
        existingUserCigar.Price = request.Price;
        existingUserCigar.Rating = request.Rating;
        existingUserCigar.HumidorId = request.HumidorId;
        existingUserCigar.UpdatedAt = DateTime.UtcNow;
        existingUserCigar.LastTouchedAt = DateTime.UtcNow;

        // Обновляем поля базовой сигары
        existingUserCigar.CigarBase.Name = request.Name;
        existingUserCigar.CigarBase.BrandId = request.BrandId;
        existingUserCigar.CigarBase.Country = request.Country;
        existingUserCigar.CigarBase.Description = request.Description;
        existingUserCigar.CigarBase.Strength = request.Strength;
        existingUserCigar.CigarBase.Size = request.Size;
        existingUserCigar.CigarBase.Wrapper = request.Wrapper;
        existingUserCigar.CigarBase.Binder = request.Binder;
        existingUserCigar.CigarBase.Filler = request.Filler;
        existingUserCigar.CigarBase.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            var oldImages = await _context.CigarImages.Where(i => i.UserCigarId == id).ToListAsync();
            _context.CigarImages.RemoveRange(oldImages);
            await TryAddUserCigarImageFromUrlAsync(id, request.ImageUrl);
        }
        else
        {
            var galleryTouched = false;
            if (request.ImageIdsToRemove is { Count: > 0 })
            {
                var toRemove = await _context.CigarImages
                    .Where(i => i.UserCigarId == id && request.ImageIdsToRemove.Contains(i.Id))
                    .ToListAsync();
                _context.CigarImages.RemoveRange(toRemove);
                galleryTouched = true;
            }

            if (request.ImageUrlsToAdd is { Count: > 0 })
            {
                await TryAppendUserCigarImagesFromUrlsAsync(id, request.ImageUrlsToAdd);
                galleryTouched = true;
            }

            if (galleryTouched)
                await EnsureUserCigarHasMainImageIfNoneAsync(id);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CigarExists(id))
                return NotFound();
            else
                throw;
        }

        // Возвращаем обновленную сигару
        var updatedCigar = await _context.UserCigars
            .Include(uc => uc.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Include(uc => uc.Humidor)
            .Where(uc => uc.Id == id)
            .Select(uc => new CigarResponseDto
            {
                Id = uc.Id,
                Name = uc.CigarBase.Name,
                Brand = new BrandDto()
                {
                    Id = uc.CigarBase.Brand.Id,
                    Name = uc.CigarBase.Brand.Name,
                    Description = uc.CigarBase.Brand.Description,
                    UpdatedAt = uc.CigarBase.Brand.UpdatedAt,
                    CreatedAt = uc.CigarBase.Brand.CreatedAt,
                    Country = uc.CigarBase.Brand.Country,
                    IsModerated = uc.CigarBase.Brand.IsModerated,
                    LogoBytes = uc.CigarBase.Brand.LogoBytes,
                },
                BrandName = uc.CigarBase.Brand.Name,
                Size = uc.CigarBase.Size,
                Strength = uc.CigarBase.Strength,
                Price = uc.Price,
                Rating = uc.Rating,
                Country = uc.CigarBase.Country,
                Description = uc.CigarBase.Description,
                Wrapper = uc.CigarBase.Wrapper,
                Binder = uc.CigarBase.Binder,
                Filler = uc.CigarBase.Filler,
                HumidorId = uc.HumidorId,
                Humidor = uc.Humidor != null ? new HumidorDto
                {
                    Id = uc.Humidor.Id,
                    Name = uc.Humidor.Name,
                    Description = uc.Humidor.Description,
                    Capacity = uc.Humidor.Capacity,
                    CreatedAt = uc.Humidor.CreatedAt,
                    UpdatedAt = uc.Humidor.UpdatedAt
                } : null,
                Images = _context.CigarImages
                    .Where(img => img.UserCigarId == uc.Id && (img.ImageData != null || img.StoragePath != null))
                    .OrderByDescending(img => img.IsMain)
                    .ThenBy(img => img.Id)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                UserId = uc.UserId,
                CreatedAt = uc.CreatedAt,
                UpdatedAt = uc.UpdatedAt,
                PurchasedAt = uc.PurchasedAt,
                SmokedAt = uc.SmokedAt,
                LastTouchedAt = uc.LastTouchedAt
            })
            .FirstOrDefaultAsync();

        return Ok(updatedCigar);
    }

    /// <summary>Скачивает одно или несколько изображений по URL; первое успешно загруженное — главное.</summary>
    private async Task TryAddUserCigarImagesFromOrderedUrlsAsync(int userCigarId, IEnumerable<string> urls)
    {
        var mainAssigned = false;
        foreach (var raw in urls)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var url = raw.Trim();
            var imageBytes = await ImageDownloader.DownloadImageAsync(url);
            if (imageBytes == null || imageBytes.Length == 0)
                continue;

            var contentType = GetContentTypeFromUrl(url);
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = ImageBinaryValidator.SuggestContentType(imageBytes);
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "image/jpeg";

            await _imageService.SaveImageAsync(
                imageData: imageBytes,
                contentType: contentType,
                fileName: GetFileNameFromUrl(url),
                description: null,
                isMain: !mainAssigned,
                cigarBaseId: null,
                userCigarId: userCigarId);
            mainAssigned = true;
        }
    }

    /// <summary>Скачивает изображение по URL и привязывает к личной сигаре (UserCigar).</summary>
    private Task TryAddUserCigarImageFromUrlAsync(int userCigarId, string? imageUrl) =>
        string.IsNullOrWhiteSpace(imageUrl)
            ? Task.CompletedTask
            : TryAddUserCigarImagesFromOrderedUrlsAsync(userCigarId, new[] { imageUrl.Trim() });

    /// <summary>Добавляет изображения по URL к уже существующим; главное не трогаем, если оно уже есть.</summary>
    private async Task TryAppendUserCigarImagesFromUrlsAsync(int userCigarId, IEnumerable<string> urls)
    {
        var list = urls.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).ToList();
        if (list.Count == 0)
            return;

        var hasMain = await _context.CigarImages.AsNoTracking()
            .AnyAsync(i => i.UserCigarId == userCigarId && i.IsMain);

        foreach (var url in list)
        {
            var imageBytes = await ImageDownloader.DownloadImageAsync(url);
            if (imageBytes == null || imageBytes.Length == 0)
                continue;

            var contentType = GetContentTypeFromUrl(url);
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = ImageBinaryValidator.SuggestContentType(imageBytes);
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "image/jpeg";

            var setMain = !hasMain;
            await _imageService.SaveImageAsync(
                imageData: imageBytes,
                contentType: contentType,
                fileName: GetFileNameFromUrl(url),
                description: null,
                isMain: setMain,
                cigarBaseId: null,
                userCigarId: userCigarId);
            if (setMain)
                hasMain = true;
        }
    }

    private async Task EnsureUserCigarHasMainImageIfNoneAsync(int userCigarId)
    {
        var imgs = await _context.CigarImages.Where(i => i.UserCigarId == userCigarId).ToListAsync();
        if (imgs.Count == 0)
            return;
        if (imgs.Exists(i => i.IsMain))
            return;
        imgs.OrderBy(i => i.Id).First().IsMain = true;
    }

    [HttpPost("{id}/smoked")]
    public async Task<ActionResult<CigarResponseDto>> MarkCigarAsSmoked(int id, [FromBody] MarkCigarSmokedRequest? request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigar = await _context.UserCigars
            .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == userId);

        if (cigar == null)
            return NotFound();

        var smokedAt = request?.SmokedAt?.ToUniversalTime() ?? DateTime.UtcNow;
        if (smokedAt > DateTime.UtcNow.AddMinutes(1))
            return BadRequest("Дата выкуривания не может быть в будущем.");

        cigar.SmokedAt = smokedAt;
        cigar.LastTouchedAt = smokedAt;
        cigar.UpdatedAt = DateTime.UtcNow;
        cigar.HumidorId = null;

        await _context.SaveChangesAsync();

        var updatedCigar = await _context.UserCigars
            .Include(uc => uc.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Include(uc => uc.Humidor)
            .Where(uc => uc.Id == id && uc.UserId == userId)
            .Select(uc => new CigarResponseDto
            {
                Id = uc.Id,
                Name = uc.CigarBase.Name,
                Brand = new BrandDto()
                {
                    Id = uc.CigarBase.Brand.Id,
                    Name = uc.CigarBase.Brand.Name,
                    Description = uc.CigarBase.Brand.Description,
                    UpdatedAt = uc.CigarBase.Brand.UpdatedAt,
                    CreatedAt = uc.CigarBase.Brand.CreatedAt,
                    Country = uc.CigarBase.Brand.Country,
                    IsModerated = uc.CigarBase.Brand.IsModerated,
                    LogoBytes = uc.CigarBase.Brand.LogoBytes,
                },
                BrandName = uc.CigarBase.Brand.Name,
                Size = uc.CigarBase.Size,
                Strength = uc.CigarBase.Strength,
                Price = uc.Price,
                Rating = uc.Rating,
                Country = uc.CigarBase.Country,
                Description = uc.CigarBase.Description,
                Wrapper = uc.CigarBase.Wrapper,
                Binder = uc.CigarBase.Binder,
                Filler = uc.CigarBase.Filler,
                HumidorId = uc.HumidorId,
                Humidor = uc.Humidor != null ? new HumidorDto
                {
                    Id = uc.Humidor.Id,
                    Name = uc.Humidor.Name,
                    Description = uc.Humidor.Description,
                    Capacity = uc.Humidor.Capacity,
                    CreatedAt = uc.Humidor.CreatedAt,
                    UpdatedAt = uc.Humidor.UpdatedAt
                } : null,
                Images = _context.CigarImages
                    .Where(img => img.UserCigarId == uc.Id && (img.ImageData != null || img.StoragePath != null))
                    .OrderByDescending(img => img.IsMain)
                    .ThenBy(img => img.Id)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                UserId = uc.UserId,
                CreatedAt = uc.CreatedAt,
                UpdatedAt = uc.UpdatedAt,
                PurchasedAt = uc.PurchasedAt,
                SmokedAt = uc.SmokedAt,
                LastTouchedAt = uc.LastTouchedAt
            })
            .FirstOrDefaultAsync();

        return Ok(updatedCigar);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCigar(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigar = await _context.UserCigars
            .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == userId);

        if (cigar == null)
            return NotFound();

        _context.UserCigars.Remove(cigar);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CigarExists(int id)
    {
        return _context.UserCigars.Any(e => e.Id == id);
    }

    [HttpPost("bases")]
    [Authorize]
    public async Task<ActionResult<CigarBaseDto>> CreateCigarBase([FromForm] CreateCigarBaseFormRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Проверяем существование бренда
        var brand = await _context.Brands.FindAsync(request.BrandId);
        if (brand == null)
        {
            return BadRequest($"Бренд с ID {request.BrandId} не найден");
        }

        // Проверяем, не существует ли уже сигара с таким названием и брендом
        var existingCigar = await _context.CigarBases
            .FirstOrDefaultAsync(cb => cb.Name.ToLower() == request.Name.ToLower() &&
                                     cb.BrandId == request.BrandId);
        if (existingCigar != null)
        {
            return BadRequest($"Сигара с названием '{request.Name}' от бренда '{brand.Name}' уже существует");
        }

        // Создаем новую базовую сигару
        var cigarBase = new CigarBase
        {
            Name = request.Name,
            BrandId = request.BrandId,
            Country = request.Country,
            Description = request.Description,
            Strength = request.Strength,
            Size = request.Size,
            Wrapper = request.Wrapper,
            Binder = request.Binder,
            Filler = request.Filler,
            IsModerated = true, // Созданные через API считаются проверенными
            CreatedAt = DateTime.UtcNow
        };

        _context.CigarBases.Add(cigarBase);
        await _context.SaveChangesAsync();

        // Загружаем файлы изображений
        if (request.NewImages != null)
        {
            foreach (var item in request.NewImages)
            {
                if (item.File is not { Length: > 0 }) continue;

                var imageBytes = await ReadFormFileAsync(item.File);
                var isMain = item.IsMain ||
                             !_context.CigarImages.Any(ci => ci.CigarBaseId == cigarBase.Id && ci.IsMain);
                await _imageService.SaveImageAsync(
                    imageData: imageBytes,
                    contentType: item.File.ContentType,
                    fileName: item.File.FileName,
                    description: null,
                    isMain: isMain,
                    cigarBaseId: cigarBase.Id,
                    userCigarId: null);
            }
        }

        // Возвращаем созданную сигару с данными
        var createdCigar = await _context.CigarBases
            .Include(cb => cb.Brand)
            .Include(cb => cb.Images)
            .Where(cb => cb.Id == cigarBase.Id)
            .Select(cb => new CigarBaseDto
            {
                Id = cb.Id,
                Name = cb.Name,
                Brand = new BrandDto
                {
                    Id = cb.BrandId,
                    Name = cb.Brand.Name,
                    Country = cb.Brand.Country,
                    Description = cb.Brand.Description,
                    LogoBytes = cb.Brand.LogoBytes,
                    IsModerated = cb.Brand.IsModerated,
                    CreatedAt = cb.Brand.CreatedAt,
                    UpdatedAt = cb.Brand.UpdatedAt
                },
                Size = cb.Size,
                Strength = cb.Strength,
                Country = cb.Country,
                Description = cb.Description,
                Wrapper = cb.Wrapper,
                Binder = cb.Binder,
                Filler = cb.Filler,
                Images = cb.Images.Where(img => img.ImageData != null || img.StoragePath != null)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                CreatedAt = cb.CreatedAt,
                UpdatedAt = cb.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetCigarBase), new { id = cigarBase.Id }, createdCigar);
    }

    [HttpGet("bases/{id}")]
    public async Task<ActionResult<CigarBaseDto>> GetCigarBase(int id)
    {
        var cigarBase = await _context.CigarBases
            .Include(cb => cb.Brand)
            .Include(cb => cb.Images)
            .Where(cb => cb.Id == id && cb.IsModerated)
            .Select(cb => new CigarBaseDto
            {
                Id = cb.Id,
                Name = cb.Name,
                Brand = new BrandDto
                {
                    Id = cb.BrandId,
                    Name = cb.Brand.Name,
                    Country = cb.Brand.Country,
                    Description = cb.Brand.Description,
                    LogoBytes = cb.Brand.LogoBytes,
                    IsModerated = cb.Brand.IsModerated,
                    CreatedAt = cb.Brand.CreatedAt,
                    UpdatedAt = cb.Brand.UpdatedAt
                },
                Size = cb.Size,
                Strength = cb.Strength,
                Country = cb.Country,
                Description = cb.Description,
                Wrapper = cb.Wrapper,
                Binder = cb.Binder,
                Filler = cb.Filler,
                Images = cb.Images.Where(img => img.ImageData != null || img.StoragePath != null)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                CreatedAt = cb.CreatedAt,
                UpdatedAt = cb.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (cigarBase == null)
            return NotFound();

        return Ok(cigarBase);
    }

    [HttpPut("bases/{id}")]
    [Authorize]
    public async Task<ActionResult<CigarBaseDto>> UpdateCigarBase(int id, [FromForm] UpdateCigarBaseFormRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var cigarBase = await _context.CigarBases
            .Include(cb => cb.Images)
            .FirstOrDefaultAsync(cb => cb.Id == id);

        if (cigarBase == null)
            return NotFound();

        // Проверяем существование бренда
        var brand = await _context.Brands.FindAsync(request.BrandId);
        if (brand == null)
        {
            return BadRequest($"Бренд с ID {request.BrandId} не найден");
        }

        // Проверяем, не существует ли уже сигара с таким названием и брендом (кроме текущей)
        var existingCigar = await _context.CigarBases
            .FirstOrDefaultAsync(cb => cb.Name.ToLower() == request.Name.ToLower() &&
                                     cb.BrandId == request.BrandId &&
                                     cb.Id != id);
        if (existingCigar != null)
        {
            return BadRequest($"Сигара с названием '{request.Name}' от бренда '{brand.Name}' уже существует");
        }

        // Обновляем поля сигары
        cigarBase.Name = request.Name;
        cigarBase.BrandId = request.BrandId;
        cigarBase.Country = request.Country;
        cigarBase.Description = request.Description;
        cigarBase.Strength = request.Strength;
        cigarBase.Size = request.Size;
        cigarBase.Wrapper = request.Wrapper;
        cigarBase.Binder = request.Binder;
        cigarBase.Filler = request.Filler;
        cigarBase.UpdatedAt = DateTime.UtcNow;

        // Загружаем новые файлы изображений
        if (request.NewImages != null)
        {
            foreach (var item in request.NewImages)
            {
                if (item.File is not { Length: > 0 }) continue;

                var imageBytes = await ReadFormFileAsync(item.File);
                var isMain = item.IsMain ||
                             !_context.CigarImages.Any(ci => ci.CigarBaseId == cigarBase.Id && ci.IsMain);
                await _imageService.SaveImageAsync(
                    imageData: imageBytes,
                    contentType: item.File.ContentType,
                    fileName: item.File.FileName,
                    description: null,
                    isMain: isMain,
                    cigarBaseId: cigarBase.Id,
                    userCigarId: null);
            }
        }

        // Удаляем изображения, если указаны
        if (request.ImageIdsToDelete != null && request.ImageIdsToDelete.Any())
        {
            var imagesToRemove = cigarBase.Images
                .Where(img => request.ImageIdsToDelete.Contains(img.Id))
                .ToList();

            foreach (var image in imagesToRemove)
            {
                await _imageService.DeleteImageAsync(image);
            }
        }

        // Обновляем флаг IsMain по состоянию, переданному фронтендом
        if (request.ExistingImages != null && request.ExistingImages.Any())
        {
            var mainId = request.ExistingImages.FirstOrDefault(e => e.IsMain)?.Id;
            if (mainId.HasValue)
            {
                foreach (var img in cigarBase.Images)
                    img.IsMain = img.Id == mainId.Value;
            }
        }

        await _context.SaveChangesAsync();

        // Возвращаем обновленную сигару с данными
        var updatedCigar = await _context.CigarBases
            .Include(cb => cb.Brand)
            .Include(cb => cb.Images)
            .Where(cb => cb.Id == cigarBase.Id)
            .Select(cb => new CigarBaseDto
            {
                Id = cb.Id,
                Name = cb.Name,
                Brand = new BrandDto
                {
                    Id = cb.BrandId,
                    Name = cb.Brand.Name,
                    Country = cb.Brand.Country,
                    Description = cb.Brand.Description,
                    LogoBytes = cb.Brand.LogoBytes,
                    IsModerated = cb.Brand.IsModerated,
                    CreatedAt = cb.Brand.CreatedAt,
                    UpdatedAt = cb.Brand.UpdatedAt
                },
                Size = cb.Size,
                Strength = cb.Strength,
                Country = cb.Country,
                Description = cb.Description,
                Wrapper = cb.Wrapper,
                Binder = cb.Binder,
                Filler = cb.Filler,
                Images = cb.Images.Where(img => img.ImageData != null || img.StoragePath != null)
                    .Select(img => new CigarImageDto
                    {
                        Id = img.Id,
                        FileName = img.FileName,
                        ContentType = img.ContentType,
                        FileSize = img.FileSize,
                        Description = img.Description,
                        IsMain = img.IsMain,
                        CigarBaseId = img.CigarBaseId,
                        UserCigarId = img.UserCigarId,
                        CreatedAt = img.CreatedAt,
                        HasThumbnail = img.ThumbnailData != null || img.ThumbnailPath != null
                    }).ToList(),
                CreatedAt = cb.CreatedAt,
                UpdatedAt = cb.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return Ok(updatedCigar);
    }

    private string GetFileNameFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            var fileName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
        }

        return Guid.NewGuid().ToString().Substring(0, 8) + ".jpg";
    }

    private string GetContentTypeFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            var extension = Path.GetExtension(uri.LocalPath).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
            }
        }

        return "image/jpeg"; // По умолчанию
    }

    private static async Task<byte[]> ReadFormFileAsync(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}
