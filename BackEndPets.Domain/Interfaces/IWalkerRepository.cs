using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkerRepository
{
    Task<Walker?> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsForUserAsync(Guid userId);
    Task<Walker> CreateAsync(Walker walker);
    Task<IReadOnlyCollection<Walker>> GetAllAsync(bool? available = null, string? workLocation = null);
    Task<Walker?> GetByIdAsync(Guid id);
    Task<Walker> UpdateAsync(Walker walker);
    Task<IReadOnlyCollection<WalkerUserProjection>> GetAllWithUserAsync(bool? available = null, string? workLocation = null);
    Task<IReadOnlyCollection<WalkerUserProjection>> GetApprovedAcceptingWithUserAsync();
    Task<WalkerUserProjection?> GetByIdWithUserAsync(Guid id);
}
