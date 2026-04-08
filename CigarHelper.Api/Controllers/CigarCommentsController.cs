using System.Security.Claims;
using CigarHelper.Api.Services;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CigarCommentsController : ControllerBase
{
    private readonly ICigarCommentService _comments;

    public CigarCommentsController(ICigarCommentService comments) => _comments = comments;

    /// <summary>Список комментариев к справочной сигаре или к чужой записи в публичной коллекции.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CigarCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CigarCommentDto>>> GetComments(
        [FromQuery] int? cigarBaseId,
        [FromQuery] int? userCigarId,
        CancellationToken cancellationToken)
    {
        if (cigarBaseId is > 0 == (userCigarId is > 0))
            return BadRequest(new { message = "Укажите ровно один параметр: cigarBaseId или userCigarId." });

        var list = await _comments.GetCommentsAsync(cigarBaseId, userCigarId, User, cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CigarCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CigarCommentDto>> Create(
        [FromBody] CreateCigarCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var created = await _comments.CreateAsync(userId, request, cancellationToken);
            if (created.CigarBaseId is int cbId)
                return CreatedAtAction(nameof(GetComments), new { cigarBaseId = cbId }, created);
            return CreatedAtAction(nameof(GetComments), new { userCigarId = created.UserCigarId }, created);
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
