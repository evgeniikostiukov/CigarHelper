using CigarHelper.Api.Models;
using CigarHelper.Api.Services;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

/// <summary>
/// Публичный просмотр профиля и хьюмидоров (только при IsProfilePublic у пользователя).
/// </summary>
[ApiController]
[Route("api/public/users")]
[AllowAnonymous]
[Produces("application/json")]
public class PublicUsersController : ControllerBase
{
    private readonly IProfileService _profileService;

    public PublicUsersController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{username}/visibility")]
    [ProducesResponseType(typeof(PublicProfileVisibilityDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicProfileVisibilityDto>> GetVisibility(string username, CancellationToken cancellationToken)
    {
        var visible = await _profileService.IsPublicProfileVisibleAsync(username, cancellationToken);
        return Ok(new PublicProfileVisibilityDto { IsVisible = visible });
    }

    [HttpGet("{username}")]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProfileDto>> GetProfile(string username, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetPublicProfileAsync(username, cancellationToken);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpGet("{username}/humidors/{humidorId:int}")]
    [ProducesResponseType(typeof(HumidorDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HumidorDetailResponseDto>> GetHumidor(string username, int humidorId, CancellationToken cancellationToken)
    {
        var detail = await _profileService.GetPublicHumidorAsync(username, humidorId, cancellationToken);
        if (detail == null)
            return NotFound();

        return Ok(detail);
    }
}
