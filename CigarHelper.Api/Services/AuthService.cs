using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public class AuthService
{
    /// <summary>Сообщение при любой неудачной попытке входа (без раскрытия, существует ли пользователь).</summary>
    public const string LoginFailedMessage = "Неверный логин или пароль.";

    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (!request.ConfirmedAge18)
        {
            return new AuthResponse
            {
                Success = false,
                Message = AuthValidationMessages.ConfirmedAge18
            };
        }

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Username already taken"
            };
        }

        // Create password hash
        JwtService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        // Create new user
        var user = new User
        {
            Username = request.Username,
            Email = null,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CreatedAt = DateTime.UtcNow
        };

        // Add user to database
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var (token, expiresAtUtc) = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            Token = token,
            Username = user.Username,
            Role = user.Role,
            Expiration = expiresAtUtc
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = LoginFailedMessage
            };
        }

        if (!JwtService.VerifyPasswordHash(
                request.Password,
                user.PasswordHash,
                user.PasswordSalt,
                out var rehashWithModernAlgorithm))
        {
            return new AuthResponse
            {
                Success = false,
                Message = LoginFailedMessage
            };
        }

        if (rehashWithModernAlgorithm)
        {
            JwtService.CreatePasswordHash(request.Password, out var newHash, out var newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
        }

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var (token, expiresAtUtc) = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            Username = user.Username,
            Role = user.Role,
            Expiration = expiresAtUtc
        };
    }

    /// <summary>По id из валидного JWT выдаёт новый токен (продление сессии).</summary>
    public async Task<AuthResponse> RefreshTokenAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User not found"
            };
        }

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var (token, expiresAtUtc) = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Token refreshed",
            Token = token,
            Username = user.Username,
            Role = user.Role,
            Expiration = expiresAtUtc
        };
    }
}