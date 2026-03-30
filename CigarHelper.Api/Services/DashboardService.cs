using CigarHelper.Data.Data;
using CigarHelper.Data.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetUserDashboardSummaryAsync(int userId);
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetUserDashboardSummaryAsync(int userId)
    {
        var humidorsQuery = _context.Humidors
            .Where(h => h.UserId == userId)
            .Select(h => new
            {
                h.Id,
                h.Capacity,
                CurrentCount = h.Cigars.Count
            });

        var humidors = await humidorsQuery.ToListAsync();

        var totalHumidors = humidors.Count;
        var totalCapacity = humidors.Sum(h => h.Capacity);
        var totalCigarsInHumidors = humidors.Sum(h => h.CurrentCount);

        var totalUserCigars = await _context.UserCigars
            .Where(c => c.UserId == userId)
            .CountAsync();

        double averageFillPercent = 0;
        if (humidors.Count > 0)
        {
            // Средняя заполненность только по хьюмидорам с Capacity > 0
            var withCapacity = humidors.Where(h => h.Capacity > 0).ToList();
            if (withCapacity.Count > 0)
            {
                var sumPercents = withCapacity
                    .Select(h => Math.Min(100d, h.CurrentCount * 100d / h.Capacity))
                    .Sum();
                averageFillPercent = sumPercents / withCapacity.Count;
            }
        }

        var brandBreakdown = await _context.UserCigars
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .GroupBy(uc => new { uc.CigarBase.BrandId, uc.CigarBase.Brand.Name })
            .Select(g => new BrandBreakdownItemDto
            {
                BrandId = g.Key.BrandId,
                BrandName = g.Key.Name,
                CigarCount = g.Count(),
                AverageRating = g.Any(uc => uc.Rating.HasValue)
                    ? g.Where(uc => uc.Rating.HasValue).Average(uc => uc.Rating)!.Value
                    : null
            })
            .OrderByDescending(x => x.CigarCount)
            .ThenBy(x => x.BrandName)
            .Take(8)
            .ToListAsync();

        var recentReviews = await _context.Reviews
            .Include(r => r.Cigar)
            .ThenInclude(uc => uc.CigarBase)
            .ThenInclude(cb => cb.Brand)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new RecentReviewDto
            {
                Id = r.Id,
                Title = r.Title,
                Rating = r.Rating,
                CigarName = r.Cigar.CigarBase.Name,
                CigarBrand = r.Cigar.CigarBase.Brand.Name,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalHumidors = totalHumidors,
            TotalCigars = totalUserCigars,
            TotalCapacity = totalCapacity,
            AverageFillPercent = Math.Round(averageFillPercent, 1),
            BrandBreakdown = brandBreakdown,
            RecentReviews = recentReviews
        };
    }
}

