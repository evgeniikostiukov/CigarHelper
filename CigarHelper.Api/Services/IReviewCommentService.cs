using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Api.Services;

public interface IReviewCommentService
{
    Task<IReadOnlyList<ReviewCommentDto>> GetCommentsAsync(
        int reviewId,
        CancellationToken cancellationToken = default);

    Task<ReviewCommentDto> CreateAsync(
        int authorUserId,
        CreateReviewCommentRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> TryDeleteAsync(
        int commentId,
        int requesterUserId,
        bool requesterIsAdmin,
        bool requesterIsModerator,
        CancellationToken cancellationToken = default);

    Task<PaginatedResult<AdminReviewCommentRowDto>> GetPendingModerationAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> TrySetModerationAsync(
        int commentId,
        CigarCommentModerationStatus status,
        int moderatorUserId,
        CancellationToken cancellationToken = default);
}
