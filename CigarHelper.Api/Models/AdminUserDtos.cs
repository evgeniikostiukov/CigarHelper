using System.ComponentModel.DataAnnotations;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Api.Models;

public class AdminUserListItemDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class PagedAdminUsersResponse
{
    public IReadOnlyList<AdminUserListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class UpdateUserRoleRequest
{
    [Required]
    public Role Role { get; set; }

    /// <summary>
    /// Обязателен при смене собственной роли (после явного подтверждения в UI).
    /// </summary>
    public bool ConfirmSelfChange { get; set; }
}

public class UpdateUserRoleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Новый JWT при смене своей роли; иначе null.
    /// </summary>
    public string? NewToken { get; set; }
}
