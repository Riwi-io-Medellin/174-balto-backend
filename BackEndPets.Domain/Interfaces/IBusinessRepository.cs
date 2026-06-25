using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IBusinessRepository
{
    Task<IReadOnlyCollection<Business>> GetByOwnerIdAsync(Guid ownerUserId);
    Task<Business> CreateAsync(Business business);
}
