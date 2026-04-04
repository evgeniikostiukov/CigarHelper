using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models.Dtos;

public class DashboardSummaryDto
{
    [Range(0, int.MaxValue)]
    public int TotalHumidors { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalCigars { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalCapacity { get; set; }

    [Range(0, 100)]
    public double AverageFillPercent { get; set; }

    [Range(0, int.MaxValue)]
    public int AverageDaysToSmoke { get; set; }

    /// <summary>Средняя оценка сигар в коллекции (UserCigars.Rating), только по позициям с заполненной оценкой.</summary>
    [Range(0, 10)]
    public double? AverageCigarRating { get; set; }

    public List<BrandBreakdownItemDto> BrandBreakdown { get; set; } = new();

    public List<RecentReviewDto> RecentReviews { get; set; } = new();

    public List<CigarTimelinePointDto> Timeline { get; set; } = new();

    public List<StaleCigarReminderDto> StaleCigarReminders { get; set; } = new();
}

public class BrandBreakdownItemDto
{
    public int BrandId { get; set; }

    [Required]
    [MaxLength(200)]
    public string BrandName { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int CigarCount { get; set; }

    [Range(0, 10)]
    public double? AverageRating { get; set; }
}

public class RecentReviewDto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CigarName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CigarBrand { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CigarTimelinePointDto
{
    [Required]
    [RegularExpression(@"^\d{4}-\d{2}$")]
    public string Period { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int PurchasedCount { get; set; }

    [Range(0, int.MaxValue)]
    public int SmokedCount { get; set; }
}

public class StaleCigarReminderDto
{
    public int CigarId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CigarName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string BrandName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DaysUntouched { get; set; }

    public DateTime LastTouchedAt { get; set; }
}

