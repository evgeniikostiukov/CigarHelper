using System.Security.Claims;
using CigarHelper.Api.Services;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

/// <summary>
/// Дашборд-сводка по коллекции текущего пользователя.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Возвращает сводную информацию по коллекции текущего пользователя.
    /// </summary>
    /// <response code="200">Успешно возвращена сводка по коллекции пользователя</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var summary = await _dashboardService.GetUserDashboardSummaryAsync(userId);
        return Ok(summary);
    }
}

