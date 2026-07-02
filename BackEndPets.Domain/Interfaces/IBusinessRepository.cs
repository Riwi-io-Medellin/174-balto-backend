using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IBusinessRepository
{
    Task<IReadOnlyCollection<Business>> GetByOwnerIdAsync(Guid ownerUserId);
    Task<Business> CreateAsync(Business business);
    Task<IReadOnlyCollection<Business>> GetAllAsync(string? type = null, string? location = null);
    Task<Business?> GetByIdAsync(Guid id);
    Task<Business> UpdateAsync(Business business);
}
