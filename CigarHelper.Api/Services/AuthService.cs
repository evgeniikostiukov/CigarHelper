using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public class AuthService
{
    /// <summary>Сообщение при любой неудачной попытке входа (без раскрытия, существует ли email).</summary>
    public const string LoginFailedMessage = "Неверный email или пароль.";

    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email already registered"
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
            Email = request.Email,
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
        // Find user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

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
}