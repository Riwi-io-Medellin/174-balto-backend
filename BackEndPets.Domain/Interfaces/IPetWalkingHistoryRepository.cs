using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IPetWalkingHistoryRepository
{
    Task<PetWalkingHistory?> GetByIdAsync(Guid historyId);
}
