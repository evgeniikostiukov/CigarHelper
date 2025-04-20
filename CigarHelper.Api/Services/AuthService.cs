using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(AppDbContext context, JwtService jwtService)
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
        
        // Generate JWT token
        var token = _jwtService.GenerateToken(user);
        
        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            Token = token,
            Username = user.Username,
            Expiration = DateTime.UtcNow.AddDays(7)
        };
    }
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        // Check if user exists
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User not found"
            };
        }
        
        // Verify password
        if (!JwtService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid password"
            };
        }
        
        // Update last login time
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        // Generate JWT token
        var token = _jwtService.GenerateToken(user);
        
        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            Username = user.Username,
            Expiration = DateTime.UtcNow.AddDays(7)
        };
    }
} 