using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public class ReviewCommentService : IReviewCommentService
{
    private const int MaxModerationPageSize = 100;

    private readonly AppDbContext _db;

    public ReviewCommentService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReviewCommentDto>> GetCommentsAsync(
        int reviewId,
        CancellationToken cancellationToken = default)
    {
        if (reviewId <= 0)
            return Array.Empty<ReviewCommentDto>();

        var reviewOk = await _db.Reviews.AsNoTracking()
            .AnyAsync(r => r.Id == reviewId && r.DeletedAt == null, cancellationToken);
        if (!reviewOk)
            return Array.Empty<ReviewCommentDto>();

        return await _db.ReviewComments.AsNoTracking()
            .Where(c => c.ReviewId == reviewId && c.ModerationStatus == CigarCommentModerationStatus.Approved)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ReviewCommentDto
            {
                Id = c.Id,
                ReviewId = c.ReviewId,
                Body = c.Body,
                CreatedAt = c.CreatedAt,
                AuthorUserId = c.AuthorUserId,
                AuthorUsername = c.Author.Username,
                IsAuthorProfilePublic = c.Author.IsProfilePublic,
                ModerationStatus = c.ModerationStatus,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ReviewCommentDto> CreateAsync(
        int authorUserId,
        CreateReviewCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = request.Body.Trim();
        if (string.IsNullOrEmpty(body))
            throw new ArgumentException("Текст комментария не может быть пустым.");

        var review = await _db.Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new ArgumentException("Обзор не найден.");

        if (review.DeletedAt != null)
            throw new ArgumentException("Нельзя комментировать удалённый обзор.");

        var author = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == authorUserId, cancellationToken);
        var authorIsStaff = author != null && (author.Role == Role.Moderator || author.Role == Role.Admin);
        var initialStatus = authorIsStaff ? CigarCommentModerationStatus.Approved : CigarCommentModerationStatus.Pending;
        var now = DateTime.UtcNow;

        var entity = new ReviewComment
        {
            ReviewId = request.ReviewId,
            AuthorUserId = authorUserId,
            Body = body,
            CreatedAt = now,
            ModerationStatus = initialStatus,
            ModeratedAt = authorIsStaff ? now : null,
            ModeratedByUserId = authorIsStaff ? authorUserId : null,
        };
        _db.ReviewComments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(entity.Id, cancellationToken);
    }

    public async Task<bool> TryDeleteAsync(
        int commentId,
        int requesterUserId,
        bool requesterIsAdmin,
        bool requesterIsModerator,
        CancellationToken cancellationToken = default)
    {
        var c = await _db.ReviewComments.FirstOrDefaultAsync(x => x.Id == commentId, cancellationToken);
        if (c == null)
            return false;

        var staff = requesterIsAdmin || requesterIsModerator;
        if (!staff && c.AuthorUserId != requesterUserId)
            return false;

        _db.ReviewComments.Remove(c);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PaginatedResult<AdminReviewCommentRowDto>> GetPendingModerationAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxModerationPageSize);

        var query = _db.ReviewComments.AsNoTracking()
            .Where(c => c.ModerationStatus == CigarCommentModerationStatus.Pending);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminReviewCommentRowDto
            {
                Id = c.Id,
                Body = c.Body,
                CreatedAt = c.CreatedAt,
                AuthorUsername = c.Author.Username,
                IsAuthorProfilePublic = c.Author.IsProfilePublic,
                ReviewId = c.ReviewId,
                TargetSummary = "Обзор: " + c.Review.Title,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AdminReviewCommentRowDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<bool> TrySetModerationAsync(
        int commentId,
        CigarCommentModerationStatus status,
        int moderatorUserId,
        CancellationToken cancellationToken = default)
    {
        if (status != CigarCommentModerationStatus.Approved && status != CigarCommentModerationStatus.Rejected)
            return false;

        var c = await _db.ReviewComments.FirstOrDefaultAsync(x => x.Id == commentId, cancellationToken);
        if (c == null || c.ModerationStatus != CigarCommentModerationStatus.Pending)
            return false;

        c.ModerationStatus = status;
        c.ModeratedAt = DateTime.UtcNow;
        c.ModeratedByUserId = moderatorUserId;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<ReviewCommentDto> MapToDtoAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.ReviewComments.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ReviewCommentDto
            {
                Id = c.Id,
                ReviewId = c.ReviewId,
                Body = c.Body,
                CreatedAt = c.CreatedAt,
                AuthorUserId = c.AuthorUserId,
                AuthorUsername = c.Author.Username,
                IsAuthorProfilePublic = c.Author.IsProfilePublic,
                ModerationStatus = c.ModerationStatus,
            })
            .FirstAsync(cancellationToken);
    }
}
