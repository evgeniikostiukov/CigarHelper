using CigarHelper.Data.Models;

namespace CigarHelper.Api.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
