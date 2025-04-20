using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public interface IReviewService
{
    Task<List<ReviewListItemDto>> GetReviewsAsync(int? userId = null, int? cigarId = null);
    Task<ReviewDto?> GetReviewByIdAsync(int id);
    Task<ReviewDto> CreateReviewAsync(int currentUserId, CreateReviewRequest request);
    Task<ReviewDto?> UpdateReviewAsync(int id, int currentUserId, UpdateReviewRequest request);
    Task<bool> DeleteReviewAsync(int id, int currentUserId);
}

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<ReviewListItemDto>> GetReviewsAsync(int? userId = null, int? cigarId = null)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Cigar)
            .Include(r => r.Images)
            .AsQueryable();
            
        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }
        
        if (cigarId.HasValue)
        {
            query = query.Where(r => r.CigarId == cigarId.Value);
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
                UserAvatarUrl = r.User.AvatarUrl,
                CigarId = r.CigarId,
                CigarName = r.Cigar.Name,
                CigarBrand = r.Cigar.Brand,
                MainImageUrl = r.Images.FirstOrDefault().ImageUrl,
                ImageCount = r.Images.Count,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }
    
    public async Task<ReviewDto?> GetReviewByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Cigar)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (review == null)
            return null;
            
        return new ReviewDto
        {
            Id = review.Id,
            Title = review.Title,
            Content = review.Content,
            Rating = review.Rating,
            UserId = review.UserId,
            Username = review.User.Username,
            UserAvatarUrl = review.User.AvatarUrl,
            CigarId = review.CigarId,
            CigarName = review.Cigar.Name,
            CigarBrand = review.Cigar.Brand,
            Images = review.Images.Select(i => new ReviewImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
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
        // Проверяем существование сигары
        var cigar = await _context.Cigars.FindAsync(request.CigarId);
        if (cigar == null)
        {
            throw new ArgumentException($"Сигара с ID {request.CigarId} не найдена");
        }
        
        var review = new Review
        {
            Title = request.Title,
            Content = request.Content,
            Rating = request.Rating,
            UserId = currentUserId,
            CigarId = request.CigarId,
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
        
        // Добавляем изображения
        if (request.Images != null && request.Images.Any())
        {
            foreach (var imageRequest in request.Images)
            {
                review.Images.Add(new ReviewImage
                {
                    ImageUrl = imageRequest.ImageUrl,
                    Caption = imageRequest.Caption,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
        
        // Получаем полный объект обзора с данными о пользователе и сигаре
        return (await GetReviewByIdAsync(review.Id))!;
    }
    
    public async Task<ReviewDto?> UpdateReviewAsync(int id, int currentUserId, UpdateReviewRequest request)
    {
        var review = await _context.Reviews
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (review == null)
            return null;
            
        // Проверяем права на редактирование
        if (review.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Вы можете редактировать только свои обзоры");
        }
        
        // Обновляем поля
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
        
        // Удаляем изображения
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
        
        // Добавляем новые изображения
        if (request.ImagesToAdd != null && request.ImagesToAdd.Any())
        {
            foreach (var imageRequest in request.ImagesToAdd)
            {
                review.Images.Add(new ReviewImage
                {
                    ImageUrl = imageRequest.ImageUrl,
                    Caption = imageRequest.Caption,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        await _context.SaveChangesAsync();
        
        // Получаем полный объект обзора с данными о пользователе и сигаре
        return await GetReviewByIdAsync(review.Id);
    }
    
    public async Task<bool> DeleteReviewAsync(int id, int currentUserId)
    {
        var review = await _context.Reviews.FindAsync(id);
        
        if (review == null)
            return false;
            
        // Проверяем права на удаление
        if (review.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Вы можете удалять только свои обзоры");
        }
        
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        
        return true;
    }
} 