using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkerRepository
{
    Task<Walker?> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsForUserAsync(Guid userId);
    Task<Walker> CreateAsync(Walker walker);
}
