using System.Security.Claims;
using CigarHelper.Api.Services;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/admin/review-comments")]
[Authorize(Roles = "Admin,Moderator")]
[Produces("application/json")]
public class AdminReviewCommentsController : ControllerBase
{
    private readonly IReviewCommentService _comments;

    public AdminReviewCommentsController(IReviewCommentService comments) => _comments = comments;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<AdminReviewCommentRowDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AdminReviewCommentRowDto>>> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _comments.GetPendingModerationAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ok = await _comments.TrySetModerationAsync(
            id,
            CigarCommentModerationStatus.Approved,
            userId,
            cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ok = await _comments.TrySetModerationAsync(
            id,
            CigarCommentModerationStatus.Rejected,
            userId,
            cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
