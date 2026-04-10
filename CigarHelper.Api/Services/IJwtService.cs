using CigarHelper.Data.Models;

namespace CigarHelper.Api.Services;

public interface IJwtService
{
    /// <summary>Выдаёт JWT и тот же момент истечения, что записан в токен (UTC).</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
