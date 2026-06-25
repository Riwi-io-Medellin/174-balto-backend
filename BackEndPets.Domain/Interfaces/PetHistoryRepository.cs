using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IPetHistoryRepository
{
    Task<PetHistory> CreateAsync(PetHistory history);
    Task<IReadOnlyCollection<PetHistory>> GetByPetIdAsync(Guid petId);
    Task<PetHistory?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}