using System.Security.Claims;
using CigarHelper.Api.Services;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewCommentsController : ControllerBase
{
    private readonly IReviewCommentService _comments;

    public ReviewCommentsController(IReviewCommentService comments) => _comments = comments;

    /// <summary>Список одобренных комментариев к обзору (только не удалённые обзоры).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ReviewCommentDto>>> GetComments(
        [FromQuery] int reviewId,
        CancellationToken cancellationToken)
    {
        if (reviewId <= 0)
            return BadRequest(new { message = "Укажите reviewId." });

        var list = await _comments.GetCommentsAsync(reviewId, cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReviewCommentDto>> Create(
        [FromBody] CreateReviewCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var created = await _comments.CreateAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetComments), new { reviewId = created.ReviewId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var isModerator = User.IsInRole("Moderator");
        var ok = await _comments.TryDeleteAsync(id, userId, isAdmin, isModerator, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
