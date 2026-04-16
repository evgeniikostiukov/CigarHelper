using CigarHelper.Api.Exceptions;
using CigarHelper.Api.Helpers;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public interface IReviewService
{
    Task<List<ReviewListItemDto>> GetReviewsAsync(int? userId = null, int? cigarBaseId = null, int? userCigarId = null);
    Task<ReviewDto?> GetReviewByIdAsync(int id);
    Task<ReviewDto> CreateReviewAsync(int currentUserId, CreateReviewRequest request);
    Task<ReviewDto?> UpdateReviewAsync(int id, int currentUserId, UpdateReviewRequest request);
    Task<bool> DeleteReviewAsync(int id, int currentUserId);
    Task<PaginatedResult<AdminDeletedReviewRowDto>> GetDeletedReviewsForStaffAsync(int page, int pageSize);
    Task<bool> RestoreReviewByStaffAsync(int id);
}

public class ReviewService : IReviewService
{
    private const int MaxStaffReviewListPageSize = 100;

    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewListItemDto>> GetReviewsAsync(int? userId = null, int? cigarBaseId = null, int? userCigarId = null)
    {
        var query = _context.Reviews
            .Where(r => r.DeletedAt == null)
            .Include(r => r.User)
            .Include(r => r.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Include(r => r.Images)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (cigarBaseId.HasValue)
        {
            query = query.Where(r => r.CigarBaseId == cigarBaseId.Value);
        }

        if (userCigarId.HasValue)
        {
            query = query.Where(r => r.CigarId == userCigarId.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewListItemDto
            {
                Id = r.Id,
                Title = r.Title,
                Summary = r.Content.Length > 200
                    ? r.Content.Substring(0, 197) + "..."
                    : r.Content,
                Rating = r.Rating,
                UserId = r.UserId,
                Username = r.User.Username,
                IsAuthorProfilePublic = r.User.IsProfilePublic,
                UserAvatarUrl = r.User.AvatarUrl == null || r.User.AvatarUrl == ""
                    ? null
                    : (r.User.AvatarUrl.ToLower().StartsWith("http://") || r.User.AvatarUrl.ToLower().StartsWith("https://"))
                        ? r.User.AvatarUrl
                        : "/api/users/" + r.User.Id.ToString() + "/avatar",
                CigarName = r.CigarBase.Name,
                CigarBrand = r.CigarBase.Brand.Name,
                CigarBaseId = r.CigarBaseId,
                MainImageBytes = r.Images.Count > 0 ? r.Images.First().ImageBytes : null,
                ImageCount = r.Images.Count,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Where(r => r.DeletedAt == null)
            .Include(r => r.User)
            .Include(r => r.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
        {
            return null;
        }

        return new ReviewDto
        {
            Id = review.Id,
            Title = review.Title,
            Content = review.Content,
            Rating = review.Rating,
            UserId = review.UserId,
            Username = review.User.Username,
            IsAuthorProfilePublic = review.User.IsProfilePublic,
            UserAvatarUrl = UserAvatarPublicUrls.ToPublicUrl(review.User.Id, review.User.AvatarUrl),
            CigarBaseId = review.CigarBaseId,
            UserCigarId = review.CigarId,
            CigarName = review.CigarBase.Name,
            CigarBrand = review.CigarBase.Brand.Name,
            Images = review.Images.Select(i => new ReviewImageDto
            {
                Id = i.Id,
                ImageBytes = i.ImageBytes,
                Caption = i.Caption
            }).ToList(),
            SmokingExperience = review.SmokingExperience,
            Aroma = review.Aroma,
            Taste = review.Taste,
            Construction = review.Construction,
            BurnQuality = review.BurnQuality,
            Draw = review.Draw,
            Venue = review.Venue,
            SmokingDate = review.SmokingDate,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public async Task<ReviewDto> CreateReviewAsync(int currentUserId, CreateReviewRequest request)
    {
        var baseExists = await _context.CigarBases.AnyAsync(cb => cb.Id == request.CigarBaseId);
        if (!baseExists)
        {
            throw new NotFoundException($"Базовая сигара с ID {request.CigarBaseId} не найдена");
        }

        int? cigarId = null;
        if (request.UserCigarId.HasValue)
        {
            var uc = await _context.UserCigars
                .FirstOrDefaultAsync(x => x.Id == request.UserCigarId.Value && x.UserId == currentUserId);
            if (uc == null)
            {
                throw new NotFoundException($"Запись коллекции с ID {request.UserCigarId} не найдена");
            }

            if (uc.CigarBaseId != request.CigarBaseId)
            {
                throw new ArgumentException("Выбранная запись коллекции не соответствует указанной базовой сигаре.");
            }

            cigarId = uc.Id;
        }

        var review = new Review
        {
            Title = request.Title,
            Content = request.Content,
            Rating = request.Rating,
            UserId = currentUserId,
            CigarBaseId = request.CigarBaseId,
            CigarId = cigarId,
            SmokingExperience = request.SmokingExperience,
            Aroma = request.Aroma,
            Taste = request.Taste,
            Construction = request.Construction,
            BurnQuality = request.BurnQuality,
            Draw = request.Draw,
            Venue = request.Venue,
            SmokingDate = request.SmokingDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        if (request.Images != null && request.Images.Any())
        {
            foreach (var imageRequest in request.Images)
            {
                var imageBytes = await ImageDownloader.DownloadImageAsync(imageRequest.ImageUrl);
                review.Images.Add(new ReviewImage
                {
                    ImageBytes = imageBytes,
                    Caption = imageRequest.Caption,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        return (await GetReviewByIdAsync(review.Id))!;
    }

    public async Task<ReviewDto?> UpdateReviewAsync(int id, int currentUserId, UpdateReviewRequest request)
    {
        var review = await _context.Reviews
            .Where(r => r.DeletedAt == null)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
        {
            return null;
        }

        if (review.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Вы можете редактировать только свои обзоры");
        }

        review.Title = request.Title;
        review.Content = request.Content;
        review.Rating = request.Rating;
        review.SmokingExperience = request.SmokingExperience;
        review.Aroma = request.Aroma;
        review.Taste = request.Taste;
        review.Construction = request.Construction;
        review.BurnQuality = request.BurnQuality;
        review.Draw = request.Draw;
        review.Venue = request.Venue;
        review.SmokingDate = request.SmokingDate ?? review.SmokingDate;
        review.UpdatedAt = DateTime.UtcNow;

        if (request.ImageIdsToRemove != null && request.ImageIdsToRemove.Any())
        {
            var imagesToRemove = review.Images
                .Where(i => request.ImageIdsToRemove.Contains(i.Id))
                .ToList();

            foreach (var image in imagesToRemove)
            {
                review.Images.Remove(image);
                _context.ReviewImages.Remove(image);
            }
        }

        if (request.ImagesToAdd != null && request.ImagesToAdd.Any())
        {
            foreach (var imageRequest in request.ImagesToAdd)
            {
                var imageBytes = await ImageDownloader.DownloadImageAsync(imageRequest.ImageUrl);
                review.Images.Add(new ReviewImage
                {
                    ImageBytes = imageBytes,
                    Caption = imageRequest.Caption,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return await GetReviewByIdAsync(review.Id);
    }

    public async Task<bool> DeleteReviewAsync(int id, int currentUserId)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return false;
        }

        if (review.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Вы можете удалять только свои обзоры");
        }

        if (review.DeletedAt != null)
        {
            return true;
        }

        review.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PaginatedResult<AdminDeletedReviewRowDto>> GetDeletedReviewsForStaffAsync(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxStaffReviewListPageSize);

        var query = _context.Reviews.AsNoTracking().Where(r => r.DeletedAt != null);
        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminDeletedReviewRowDto
            {
                Id = r.Id,
                Title = r.Title,
                UserId = r.UserId,
                Username = r.User.Username,
                IsAuthorProfilePublic = r.User.IsProfilePublic,
                CigarBaseId = r.CigarBaseId,
                CigarName = r.CigarBase.Name,
                CigarBrand = r.CigarBase.Brand.Name,
                CreatedAt = r.CreatedAt,
                DeletedAt = r.DeletedAt
            })
            .ToListAsync();

        return new PaginatedResult<AdminDeletedReviewRowDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<bool> RestoreReviewByStaffAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null || review.DeletedAt == null)
        {
            return false;
        }

        review.DeletedAt = null;
        await _context.SaveChangesAsync();
        return true;
    }
}
