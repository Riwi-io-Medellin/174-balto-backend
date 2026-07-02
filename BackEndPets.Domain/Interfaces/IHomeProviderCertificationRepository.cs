using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderCertificationRepository
{
    Task<HomeProviderCertification> CreateAsync(HomeProviderCertification certification);
    Task<IReadOnlyCollection<HomeProviderCertification>> GetByProviderIdAsync(Guid providerId);
    Task<HomeProviderCertification?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
