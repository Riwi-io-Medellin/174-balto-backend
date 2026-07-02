using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeProviderServicesService
{
    Task<(HomeProviderServiceResponse? Service, string? ErrorCode)> AddAsync(
        Guid userId, AddHomeProviderServiceRequest request);
    Task<IReadOnlyCollection<HomeProviderServiceResponse>> GetByProviderIdAsync(Guid providerId);
    Task<(HomeProviderServiceResponse? Service, string? ErrorCode)> UpdateAsync(
        Guid userId, Guid serviceId, UpdateHomeProviderServiceRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid serviceId);
}
