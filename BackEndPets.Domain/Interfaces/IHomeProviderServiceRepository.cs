using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderServiceRepository
{
    Task<HomeProviderService> CreateAsync(HomeProviderService service);
    Task<IReadOnlyCollection<HomeProviderService>> GetByProviderIdAsync(Guid providerId);
    Task<HomeProviderService?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid providerId, Guid serviceTypeId);
    Task<HomeProviderService?> UpdateAsync(HomeProviderService service);
    Task<bool> DeleteAsync(Guid id);
}
