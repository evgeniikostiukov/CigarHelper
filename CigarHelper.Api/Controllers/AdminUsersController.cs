using System.Security.Claims;
using CigarHelper.Api.Models;
using CigarHelper.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    /// <summary>
    /// Список пользователей (пагинация и поиск по имени или email).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedAdminUsersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedAdminUsersResponse>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminUserService.GetUsersAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Изменение роли пользователя. При смене своей роли клиент должен передать confirmSelfChange после подтверждения в UI; в ответе может быть newToken.
    /// </summary>
    [HttpPut("{userId:int}/role")]
    [ProducesResponseType(typeof(UpdateUserRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UpdateUserRoleResponse>> UpdateRole(
        [FromRoute] int userId,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var actorId = GetUserId();
        if (actorId == null)
            return Unauthorized();

        var result = await _adminUserService.UpdateUserRoleAsync(actorId.Value, userId, request.Role, request.ConfirmSelfChange, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("id");
        return int.TryParse(id, out var parsed) ? parsed : null;
    }
}
