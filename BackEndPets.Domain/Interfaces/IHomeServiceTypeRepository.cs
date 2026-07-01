using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeServiceTypeRepository
{
    Task<IReadOnlyCollection<HomeServiceType>> GetAllActiveAsync();
    Task<HomeServiceType?> GetByIdAsync(Guid id);
    Task<HomeServiceType?> GetByCodeAsync(string code);
}
