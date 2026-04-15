using CigarHelper.Api.Services;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin,Moderator")]
[Produces("application/json")]
public class AdminReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public AdminReviewsController(IReviewService reviews) => _reviews = reviews;

    /// <summary>Список обзоров, мягко удалённых авторами (для восстановления).</summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PaginatedResult<AdminDeletedReviewRowDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AdminDeletedReviewRowDto>>> GetDeleted(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _reviews.GetDeletedReviewsForStaffAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Восстановить обзор (снять мягкое удаление).</summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(int id)
    {
        var ok = await _reviews.RestoreReviewByStaffAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
