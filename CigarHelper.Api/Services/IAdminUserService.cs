using CigarHelper.Api.Models;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Api.Services;

public interface IAdminUserService
{
    Task<PagedAdminUsersResponse> GetUsersAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

    Task<UpdateUserRoleResponse> UpdateUserRoleAsync(int actorUserId, int targetUserId, Role newRole, bool confirmSelfChange, CancellationToken cancellationToken = default);
}
