using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IPetWalkingHistoryRepository
{
    Task<PetWalkingHistory> CreateAsync(PetWalkingHistory history);
    Task<PetWalkingHistory?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<PetWalkingHistory>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyCollection<PetWalkingHistory>> GetByWalkerIdAsync(Guid walkerId);
    Task<IReadOnlyCollection<PetWalkingHistory>> GetBySessionIdAsync(Guid sessionId);
    Task UpdateAsync(PetWalkingHistory history);
}