using CigarHelper.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

/// <summary>Отдача аватаров по ключу в <c>Users.AvatarUrl</c> (не для внешних URL).</summary>
[ApiController]
[Route("api/users")]
[AllowAnonymous]
public class UserAvatarsController : ControllerBase
{
    private readonly IUserAvatarService _avatars;

    public UserAvatarsController(IUserAvatarService avatars)
    {
        _avatars = avatars;
    }

    [HttpGet("{id:int}/avatar")]
    public async Task<IActionResult> GetAvatar(int id, CancellationToken cancellationToken)
    {
        var (data, contentType) = await _avatars.GetStoredAvatarAsync(id, cancellationToken);
        if (data == null || data.Length == 0)
            return NotFound();

        return File(data, contentType);
    }
}
