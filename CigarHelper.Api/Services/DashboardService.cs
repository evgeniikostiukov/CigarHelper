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
        var now = DateTime.UtcNow;
        var humidorsQuery = _context.Humidors
            .Where(h => h.UserId == userId)
            .Select(h => new
            {
                h.Id,
                h.Capacity,
                CurrentCount = h.Cigars.Sum(c => (int?)c.Quantity) ?? 0
            });

        var humidors = await humidorsQuery.ToListAsync();

        var totalHumidors = humidors.Count;
        var totalCapacity = humidors.Sum(h => h.Capacity);
        var totalCigarsInHumidors = humidors.Sum(h => h.CurrentCount);

        var totalUserCigars = await _context.UserCigars
            .Where(c => c.UserId == userId)
            .SumAsync(c => (int?)c.Quantity) ?? 0;

        var ratedCigarsQuery = _context.UserCigars
            .Where(uc => uc.UserId == userId && uc.Rating.HasValue);
        double? averageCigarRating = null;
        if (await ratedCigarsQuery.AnyAsync())
        {
            var avg = await ratedCigarsQuery.AverageAsync(uc => (double)uc.Rating!.Value);
            averageCigarRating = Math.Round(avg, 1);
        }

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
                CigarCount = g.Sum(x => x.Quantity),
                AverageRating = g.Any(uc => uc.Rating.HasValue)
                    ? g.Where(uc => uc.Rating.HasValue).Average(uc => uc.Rating)!.Value
                    : null
            })
            .OrderByDescending(x => x.CigarCount)
            .ThenBy(x => x.BrandName)
            .Take(8)
            .ToListAsync();

        var recentReviews = await _context.Reviews
            .Where(r => r.UserId == userId && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new RecentReviewDto
            {
                Id = r.Id,
                Title = r.Title,
                Rating = r.Rating,
                CigarName = r.CigarBase.Name,
                CigarBrand = r.CigarBase.Brand.Name,
                CreatedAt = r.CreatedAt,
                Username = r.User.Username,
                IsAuthorProfilePublic = r.User.IsProfilePublic,
                UserAvatarUrl = r.User.AvatarUrl == null || r.User.AvatarUrl == ""
                    ? null
                    : (r.User.AvatarUrl.ToLower().StartsWith("http://") || r.User.AvatarUrl.ToLower().StartsWith("https://"))
                        ? r.User.AvatarUrl
                        : "/api/users/" + r.User.Id.ToString() + "/avatar"
            })
            .ToListAsync();

        var lifecycle = await _context.UserCigars
            .Where(uc => uc.UserId == userId)
            .Select(uc => new
            {
                uc.Id,
                uc.PurchasedAt,
                uc.SmokedAt,
                uc.LastTouchedAt,
                CigarName = uc.CigarBase.Name,
                BrandName = uc.CigarBase.Brand.Name
            })
            .ToListAsync();

        var smokedDurations = lifecycle
            .Where(x => x.SmokedAt.HasValue && x.PurchasedAt <= x.SmokedAt.Value)
            .Select(x => (x.SmokedAt!.Value.Date - x.PurchasedAt.Date).Days)
            .Where(days => days >= 0)
            .ToList();

        var averageDaysToSmoke = smokedDurations.Count == 0
            ? 0
            : (int)Math.Round(smokedDurations.Average(), MidpointRounding.AwayFromZero);

        var timelineStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
        var timeline = Enumerable
            .Range(0, 6)
            .Select(i => timelineStart.AddMonths(i))
            .Select(monthStart =>
            {
                var nextMonth = monthStart.AddMonths(1);
                return new CigarTimelinePointDto
                {
                    Period = monthStart.ToString("yyyy-MM"),
                    PurchasedCount = lifecycle.Count(x => x.PurchasedAt >= monthStart && x.PurchasedAt < nextMonth),
                    SmokedCount = lifecycle.Count(x => x.SmokedAt.HasValue && x.SmokedAt.Value >= monthStart && x.SmokedAt.Value < nextMonth)
                };
            })
            .ToList();

        const int staleThresholdDays = 45;
        var staleReminders = lifecycle
            .Where(x => !x.SmokedAt.HasValue)
            .Select(x =>
            {
                var touchPoint = x.LastTouchedAt > x.PurchasedAt ? x.LastTouchedAt : x.PurchasedAt;
                var daysUntouched = (now.Date - touchPoint.Date).Days;
                return new StaleCigarReminderDto
                {
                    CigarId = x.Id,
                    CigarName = x.CigarName,
                    BrandName = x.BrandName,
                    LastTouchedAt = touchPoint,
                    DaysUntouched = daysUntouched
                };
            })
            .Where(x => x.DaysUntouched >= staleThresholdDays)
            .OrderByDescending(x => x.DaysUntouched)
            .ThenBy(x => x.CigarName)
            .Take(5)
            .ToList();

        return new DashboardSummaryDto
        {
            TotalHumidors = totalHumidors,
            TotalCigars = totalUserCigars,
            TotalCapacity = totalCapacity,
            AverageFillPercent = Math.Round(averageFillPercent, 1),
            AverageDaysToSmoke = averageDaysToSmoke,
            AverageCigarRating = averageCigarRating,
            BrandBreakdown = brandBreakdown,
            RecentReviews = recentReviews,
            Timeline = timeline,
            StaleCigarReminders = staleReminders
        };
    }
}

