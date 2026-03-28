using CigarHelper.Api.Models;
using CigarHelper.Data.Models.Dtos;

namespace CigarHelper.Api.Services;

public interface IProfileService
{
    Task<MyProfileDto?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UpdateProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ChangePasswordResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<PublicProfileDto?> GetPublicProfileAsync(string username, CancellationToken cancellationToken = default);
    Task<HumidorDetailResponseDto?> GetPublicHumidorAsync(string username, int humidorId, CancellationToken cancellationToken = default);
}
