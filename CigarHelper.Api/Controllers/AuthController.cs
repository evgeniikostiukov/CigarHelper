using System.Security.Claims;
using CigarHelper.Data.Models;
using CigarHelper.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <returns>Информация о созданном пользователе и JWT токен</returns>
    /// <response code="200">Регистрация успешна</response>
    /// <response code="400">Ошибка валидации или пользователь уже существует</response>
    [HttpPost("register")]
    [EnableRateLimiting("auth-register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { message = "Validation failed", errors });
        }

        var response = await _authService.RegisterAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Авторизует пользователя и выдает JWT токен
    /// </summary>
    /// <param name="request">Учетные данные для входа</param>
    /// <returns>Информация о пользователе и JWT токен</returns>
    /// <response code="200">Авторизация успешна</response>
    /// <response code="401">Некорректные учетные данные</response>
    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { message = "Validation failed", errors });
        }

        var response = await _authService.LoginAsync(request);

        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }

    /// <summary>
    /// Выдаёт новый JWT по действующему Bearer-токену (продление сессии при повторном открытии клиента).
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    [EnableRateLimiting("auth-refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
        if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var userId))
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Invalid token claims"
            });
        }

        var response = await _authService.RefreshTokenAsync(userId);

        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }
}