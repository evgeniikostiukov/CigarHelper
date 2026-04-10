using System.Security.Claims;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Api.Services;

public interface ICigarCommentService
{
    Task<IReadOnlyList<CigarCommentDto>> GetCommentsAsync(
        int? cigarBaseId,
        int? userCigarId,
        ClaimsPrincipal? user,
        CancellationToken cancellationToken = default);

    Task<CigarCommentDto> CreateAsync(
        int authorUserId,
        CreateCigarCommentRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> TryDeleteAsync(
        int commentId,
        int requesterUserId,
        bool requesterIsAdmin,
        bool requesterIsModerator,
        CancellationToken cancellationToken = default);

    Task<PaginatedResult<AdminCigarCommentRowDto>> GetPendingModerationAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> TrySetModerationAsync(
        int commentId,
        CigarCommentModerationStatus status,
        int moderatorUserId,
        CancellationToken cancellationToken = default);
}
