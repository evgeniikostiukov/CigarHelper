using CigarHelper.Api.Models;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Api.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CigarHelper.Api.Services;

public class ProfileService : IProfileService
{
    private const string PasswordChangeCachePrefix = "profile_pwd_change_";
    private static readonly TimeSpan PasswordChangeMinInterval = TimeSpan.FromHours(1);

    private readonly AppDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly IHumidorService _humidorService;
    private readonly IMemoryCache _cache;

    public ProfileService(
        AppDbContext db,
        IJwtService jwtService,
        IHumidorService humidorService,
        IMemoryCache cache)
    {
        _db = db;
        _jwtService = jwtService;
        _humidorService = humidorService;
        _cache = cache;
    }

    public async Task<MyProfileDto?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        return u == null ? null : MapToMyProfile(u);
    }

    public async Task<UpdateProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
        {
            return new UpdateProfileResponse { Success = false, Message = "Пользователь не найден" };
        }

        var usernameNorm = request.Username.Trim();
        var emailNorm = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        if (!string.Equals(user.Username, usernameNorm, StringComparison.Ordinal))
        {
            var taken = await _db.Users.AnyAsync(x => x.Username == usernameNorm && x.Id != userId, cancellationToken);
            if (taken)
            {
                return new UpdateProfileResponse { Success = false, Message = "Имя пользователя уже занято" };
            }
        }

        if (emailNorm != null
            && !string.Equals(user.Email, emailNorm, StringComparison.OrdinalIgnoreCase))
        {
            var taken = await _db.Users.AnyAsync(x => x.Email == emailNorm && x.Id != userId, cancellationToken);
            if (taken)
            {
                return new UpdateProfileResponse { Success = false, Message = "Этот email уже зарегистрирован" };
            }
        }

        var needNewToken = !string.Equals(user.Username, usernameNorm, StringComparison.Ordinal)
            || !string.Equals(user.Email ?? "", emailNorm ?? "", StringComparison.OrdinalIgnoreCase);

        user.Username = usernameNorm;
        user.Email = emailNorm;
        user.IsProfilePublic = request.IsProfilePublic;

        await _db.SaveChangesAsync(cancellationToken);

        string? newToken = null;
        if (needNewToken)
        {
            var (token, _) = _jwtService.GenerateToken(user);
            newToken = token;
        }

        return new UpdateProfileResponse
        {
            Success = true,
            Message = "Профиль обновлён",
            Profile = MapToMyProfile(user),
            NewToken = newToken,
        };
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var cacheKey = PasswordChangeCachePrefix + userId;
        if (_cache.TryGetValue(cacheKey, out DateTime lastSuccessUtc))
        {
            var nextAllowed = lastSuccessUtc + PasswordChangeMinInterval;
            if (DateTime.UtcNow < nextAllowed)
            {
                var sec = (int)Math.Ceiling((nextAllowed - DateTime.UtcNow).TotalSeconds);
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "Смена пароля доступна не чаще одного раза в час. Повторите позже.",
                    RetryAfterSeconds = Math.Max(1, sec),
                };
            }
        }

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
        {
            return new ChangePasswordResponse { Success = false, Message = "Пользователь не найден" };
        }

        if (!JwtService.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return new ChangePasswordResponse { Success = false, Message = "Неверный текущий пароль" };
        }

        JwtService.CreatePasswordHash(request.NewPassword, out var hash, out var salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await _db.SaveChangesAsync(cancellationToken);

        _cache.Set(
            cacheKey,
            DateTime.UtcNow,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
            });

        return new ChangePasswordResponse { Success = true, Message = "Пароль изменён" };
    }

    public async Task<bool> IsPublicProfileVisibleAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        return user != null && user.IsProfilePublic;
    }

    public async Task<PublicProfileDto?> GetPublicProfileAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        if (user == null || !user.IsProfilePublic)
        {
            return null;
        }

        var humidors = await _humidorService.GetUserHumidors(user.Id);

        return new PublicProfileDto
        {
            Username = user.Username,
            AvatarUrl = UserAvatarPublicUrls.ToPublicUrl(user.Id, user.AvatarUrl),
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            Humidors = humidors,
        };
    }

    public async Task<HumidorDetailResponseDto?> GetPublicHumidorAsync(string username, int humidorId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        if (user == null || !user.IsProfilePublic)
        {
            return null;
        }

        return await _humidorService.GetHumidorById(humidorId, user.Id);
    }

    private static MyProfileDto MapToMyProfile(User u) => MyProfileDto.FromUser(u);
}
