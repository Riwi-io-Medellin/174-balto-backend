using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IPetRepository
{
    Task<Pet> CreateAsync(Pet pet);
    Task<Pet?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<Pet>> GetByUserIdAsync(Guid userId);
    Task<Pet?> UpdateAsync(Pet pet);
    Task<bool> DeleteAsync(Guid id);
}