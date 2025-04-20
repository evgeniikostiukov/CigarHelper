using CigarHelper.Data.Models;
using CigarHelper.Api.Services;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        // Debug - вывод данных запроса
        Console.WriteLine($"Register request received: Username={request.Username}, Email={request.Email}, " +
                         $"Password Length={request.Password?.Length ?? 0}, ConfirmPassword Length={request.ConfirmPassword?.Length ?? 0}");
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            Console.WriteLine($"Registration validation failed: {System.Text.Json.JsonSerializer.Serialize(errors)}");
            return BadRequest(new { message = "Validation failed", errors });
        }
            
        var response = await _authService.RegisterAsync(request);
        
        if (!response.Success)
        {
            Console.WriteLine($"Registration failed: {response.Message}");
            return BadRequest(response);
        }
            
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
} 