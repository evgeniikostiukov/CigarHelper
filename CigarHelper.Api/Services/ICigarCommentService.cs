using System.Security.Claims;
using CigarHelper.Data.Models.Dtos;

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
        CancellationToken cancellationToken = default);
}
