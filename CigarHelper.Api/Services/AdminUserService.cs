using CigarHelper.Api.Models;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public class AdminUserService : IAdminUserService
{
    private const int MaxPageSize = 100;

    private readonly AppDbContext _db;
    private readonly IJwtService _jwtService;

    public AdminUserService(AppDbContext db, IJwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    public async Task<PagedAdminUsersResponse> GetUsersAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Username.Contains(term) ||
                u.Email.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
            })
            .ToListAsync(cancellationToken);

        return new PagedAdminUsersResponse
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<UpdateUserRoleResponse> UpdateUserRoleAsync(int actorUserId, int targetUserId, Role newRole, bool confirmSelfChange, CancellationToken cancellationToken = default)
    {
        var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);
        if (target == null)
        {
            return new UpdateUserRoleResponse { Success = false, Message = "Пользователь не найден" };
        }

        if (target.Role == newRole)
        {
            return new UpdateUserRoleResponse { Success = true, Message = "Роль без изменений" };
        }

        var isSelf = actorUserId == targetUserId;
        if (isSelf && !confirmSelfChange)
        {
            return new UpdateUserRoleResponse
            {
                Success = false,
                Message = "Требуется подтверждение смены собственной роли",
            };
        }

        if (target.Role == Role.Admin && newRole != Role.Admin)
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == Role.Admin, cancellationToken);
            if (adminCount <= 1)
            {
                return new UpdateUserRoleResponse
                {
                    Success = false,
                    Message = "Нельзя снять роль администратора с последнего администратора",
                };
            }
        }

        target.Role = newRole;
        await _db.SaveChangesAsync(cancellationToken);

        string? newToken = null;
        if (isSelf)
        {
            var refreshed = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == actorUserId, cancellationToken);
            newToken = _jwtService.GenerateToken(refreshed);
        }

        return new UpdateUserRoleResponse
        {
            Success = true,
            Message = "Роль обновлена",
            NewToken = newToken,
        };
    }
}
