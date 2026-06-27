using BackEndPets.Application.DTOs.Businesses;

namespace BackEndPets.Application.Interfaces;

public interface IBusinessServicesService
{
    Task<(BusinessServiceResponse? Service, string? ErrorCode)> CreateAsync(
        Guid userId, Guid businessId, CreateBusinessServiceRequest request);
    Task<IReadOnlyCollection<BusinessServiceResponse>> GetByBusinessIdAsync(Guid businessId);
    Task<(BusinessServiceResponse? Service, string? ErrorCode)> UpdateAsync(
        Guid userId, Guid serviceId, UpdateBusinessServiceRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid serviceId);
}