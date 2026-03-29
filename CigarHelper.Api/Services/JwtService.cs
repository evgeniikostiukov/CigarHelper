using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CigarHelper.Data.Models;
using Microsoft.IdentityModel.Tokens;

namespace CigarHelper.Api.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured"));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("id", user.Id.ToString()), // Добавляем явное поле id
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// PBKDF2-HMAC-SHA256; число итераций по рекомендациям OWASP Password Storage.
    /// </summary>
    public const int Pbkdf2Iterations = 310_000;

    private const int Pbkdf2SaltSize = 16;
    private const int Pbkdf2HashSize = 32;

    private const int LegacyHmacHashSize = 64;

    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        passwordSalt = new byte[Pbkdf2SaltSize];
        RandomNumberGenerator.Fill(passwordSalt);
        passwordHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            passwordSalt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            Pbkdf2HashSize);
    }

    public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt) =>
        VerifyPasswordHash(password, passwordHash, passwordSalt, out _);

    /// <summary>Проверяет PBKDF2-HMAC-SHA256 или устаревший HMAC-SHA512 (длина хеша 64 байта).</summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <param name="passwordHash">Сохранённый хеш (32 байта PBKDF2 или 64 байта HMAC-SHA512).</param>
    /// <param name="passwordSalt">Соль (16 байт PBKDF2 или ключ HMAC из старого кода).</param>
    /// <param name="rehashWithModernAlgorithm">После HMAC-успеха — true, чтобы при логине сохранить PBKDF2.</param>
    public static bool VerifyPasswordHash(
        string password,
        byte[] passwordHash,
        byte[] passwordSalt,
        out bool rehashWithModernAlgorithm)
    {
        rehashWithModernAlgorithm = false;

        if (passwordHash.Length == 0 || passwordSalt.Length == 0)
            return false;

        if (passwordHash.Length == Pbkdf2HashSize && passwordSalt.Length == Pbkdf2SaltSize)
        {
            var expected = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                passwordSalt,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                Pbkdf2HashSize);
            return CryptographicOperations.FixedTimeEquals(expected, passwordHash);
        }

        // Старый формат: один HMACSHA512 по паролю; длина хеша SHA-512 = 64 байта; ключ — случайный Key из ctor().
        if (passwordHash.Length == LegacyHmacHashSize && passwordSalt.Length > 0)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var ok = CryptographicOperations.FixedTimeEquals(computed, passwordHash);
            if (ok)
                rehashWithModernAlgorithm = true;
            return ok;
        }

        return false;
    }
}