using CigarHelper.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

/// <summary>
/// Денормализованные средние по осям отзывов на <see cref="CigarHelper.Data.Models.CigarBase"/>:
/// среднее только по отзывам с заполненной соответствующей осью; счётчик — отзывы с хотя бы одной осью.
/// </summary>
public static class CigarBaseReviewStatsRefresher
{
    public static async Task RefreshAsync(AppDbContext context, int cigarBaseId, CancellationToken cancellationToken = default)
    {
        var cigarBase = await context.CigarBases
            .FirstOrDefaultAsync(b => b.Id == cigarBaseId, cancellationToken);
        if (cigarBase == null)
            return;

        var q = context.Reviews.AsNoTracking()
            .Where(r => r.CigarBaseId == cigarBaseId && r.DeletedAt == null);

        if (await q.AnyAsync(r => r.BodyStrengthScore != null, cancellationToken))
        {
            cigarBase.ReviewAvgBodyStrength = await q.Where(r => r.BodyStrengthScore != null)
                .AverageAsync(r => (decimal)r.BodyStrengthScore!, cancellationToken);
        }
        else
        {
            cigarBase.ReviewAvgBodyStrength = null;
        }

        if (await q.AnyAsync(r => r.AromaScore != null, cancellationToken))
        {
            cigarBase.ReviewAvgAromaScore = await q.Where(r => r.AromaScore != null)
                .AverageAsync(r => (decimal)r.AromaScore!, cancellationToken);
        }
        else
        {
            cigarBase.ReviewAvgAromaScore = null;
        }

        if (await q.AnyAsync(r => r.PairingsScore != null, cancellationToken))
        {
            cigarBase.ReviewAvgPairingsScore = await q.Where(r => r.PairingsScore != null)
                .AverageAsync(r => (decimal)r.PairingsScore!, cancellationToken);
        }
        else
        {
            cigarBase.ReviewAvgPairingsScore = null;
        }

        cigarBase.ReviewScoredReviewCount = await q.CountAsync(
            r => r.BodyStrengthScore != null || r.AromaScore != null || r.PairingsScore != null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
