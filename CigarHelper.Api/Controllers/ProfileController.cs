using System.Security.Claims;
using CigarHelper.Api.Models;
using CigarHelper.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IUserAvatarService _userAvatarService;

    public ProfileController(IProfileService profileService, IUserAvatarService userAvatarService)
    {
        _profileService = profileService;
        _userAvatarService = userAvatarService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(MyProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MyProfileDto>> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var profile = await _profileService.GetMyProfileAsync(userId.Value, cancellationToken);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpPatch]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UpdateProfileResponse>> Patch([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        TryValidateModel(request);
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _profileService.UpdateProfileAsync(userId.Value, request, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        TryValidateModel(request);
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _profileService.ChangePasswordAsync(userId.Value, request, cancellationToken);
        if (!result.Success)
        {
            if (result.RetryAfterSeconds is > 0)
            {
                Response.Headers.RetryAfter = result.RetryAfterSeconds.Value.ToString();
                return StatusCode(StatusCodes.Status429TooManyRequests, result);
            }

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>Загрузка аватара (JPEG/PNG/WebP/AVIF/GIF), сохраняется как WebP до 384 px по длинной стороне.</summary>
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MyProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MyProfileDto>> UploadAvatar(IFormFile? file, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Выберите файл изображения." });

        await using var stream = file.OpenReadStream();
        var (ok, err) = await _userAvatarService.UploadAsync(
            userId.Value,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        if (!ok)
            return BadRequest(new { message = err ?? "Не удалось загрузить аватар." });

        var profile = await _profileService.GetMyProfileAsync(userId.Value, cancellationToken);
        return profile == null ? NotFound() : Ok(profile);
    }

    /// <summary>Удалить загруженный аватар (внешний URL в поле не трогаем — только сброс ключа в хранилище).</summary>
    [HttpDelete("avatar")]
    [ProducesResponseType(typeof(MyProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyProfileDto>> DeleteAvatar(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var (ok, err) = await _userAvatarService.ClearAsync(userId.Value, cancellationToken);
        if (!ok)
            return BadRequest(new { message = err ?? "Не удалось удалить аватар." });

        var profile = await _profileService.GetMyProfileAsync(userId.Value, cancellationToken);
        return profile == null ? NotFound() : Ok(profile);
    }

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("id");
        return int.TryParse(id, out var parsed) ? parsed : null;
    }
}
