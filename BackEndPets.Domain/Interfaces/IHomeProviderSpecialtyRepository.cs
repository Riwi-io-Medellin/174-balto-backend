using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderSpecialtyRepository
{
    Task<HomeProviderSpecialty> CreateAsync(HomeProviderSpecialty specialty);
    Task<IReadOnlyCollection<HomeProviderSpecialty>> GetByProviderIdAsync(Guid providerId);
    Task<HomeProviderSpecialty?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid providerId, string specialty);
    Task<bool> DeleteAsync(Guid id);
}
