using BackEndPets.Application.DTOs.Profiles;

namespace BackEndPets.Application.Interfaces;

public interface IProfileService
{
    Task<MeResponse?> GetMeAsync(Guid userId);
    Task<(WalkerResponse? Walker, string? ErrorCode)> BecomeWalkerAsync(Guid userId);
    Task<(BusinessResponse? Business, string? ErrorCode)> CreateBusinessAsync(Guid userId, CreateBusinessRequest request);
    Task<IReadOnlyCollection<WalkerResponse>> GetWalkersAsync(bool? available = null, string? workLocation = null);
    Task<WalkerResponse?> GetWalkerByIdAsync(Guid id);
    Task<IReadOnlyCollection<BusinessResponse>> GetBusinessesAsync(string? type = null, string? location = null);
    Task<BusinessResponse?> GetBusinessByIdAsync(Guid id);
    Task<IReadOnlyCollection<WalkerRecommendationResponse>> GetWalkerRecommendationsAsync(
        WalkerRecommendationRequest request);
    Task<(WalkerProfileResponse? Profile, string? ErrorCode)> GetMyWalkerProfileAsync(Guid userId);
    Task<(WalkerProfileResponse? Profile, string? ErrorCode)> UpdateMyWalkerProfileAsync(
        Guid userId, UpdateWalkerProfileRequest request);
    Task<(BusinessResponse? Business, string? ErrorCode)> UpdateMyBusinessAsync(
        Guid userId, UpdateBusinessRequest request);
}
