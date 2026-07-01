using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class FavoriteHomeProviderRepository(AppIdentityDbContext dbContext) : IFavoriteHomeProviderRepository
{
    public async Task<FavoriteHomeProvider> AddAsync(FavoriteHomeProvider favorite)
    {
        favorite.CreatedAt = DateTime.UtcNow;
        dbContext.FavoriteHomeProviders.Add(favorite);
        await dbContext.SaveChangesAsync();
        return favorite;
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid providerId)
    {
        var existing = await dbContext.FavoriteHomeProviders
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProviderId == providerId);
        if (existing is null) return false;
        dbContext.FavoriteHomeProviders.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid userId, Guid providerId) =>
        dbContext.FavoriteHomeProviders.AnyAsync(f => f.UserId == userId && f.ProviderId == providerId);

    public async Task<IReadOnlyCollection<FavoriteHomeProvider>> GetByUserIdAsync(Guid userId) =>
        await dbContext.FavoriteHomeProviders
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
}
