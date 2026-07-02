using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeServiceProviderService
{
    Task<(HomeServiceProviderResponse? Provider, string? ErrorCode)> BecomeProviderAsync(Guid userId);
    Task<IReadOnlyCollection<HomeServiceProviderResponse>> GetProvidersAsync(HomeServiceProviderFilterRequest filters);
    Task<HomeServiceProviderResponse?> GetProviderByIdAsync(Guid id);
    Task<(HomeServiceProviderProfileResponse? Profile, string? ErrorCode)> GetMyProviderProfileAsync(Guid userId);
    Task<(HomeServiceProviderProfileResponse? Profile, string? ErrorCode)> UpdateMyProviderProfileAsync(
        Guid userId, UpdateHomeServiceProviderRequest request);
}
