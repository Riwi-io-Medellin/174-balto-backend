using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IFavoriteHomeProviderRepository
{
    Task<FavoriteHomeProvider> AddAsync(FavoriteHomeProvider favorite);
    Task<bool> RemoveAsync(Guid userId, Guid providerId);
    Task<bool> ExistsAsync(Guid userId, Guid providerId);
    Task<IReadOnlyCollection<FavoriteHomeProvider>> GetByUserIdAsync(Guid userId);
}
