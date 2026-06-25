using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkerGalleryRepository(AppIdentityDbContext dbContext) : IWalkerGalleryRepository
{
    public async Task<WalkerGallery> CreateAsync(WalkerGallery photo)
    {
        photo.Id = Guid.NewGuid();
        photo.CreatedAt = DateTime.UtcNow;
        dbContext.WalkerGalleries.Add(photo);
        await dbContext.SaveChangesAsync();
        return photo;
    }

    public async Task<IReadOnlyCollection<WalkerGallery>> GetByWalkerIdAsync(Guid walkerId) =>
        await dbContext.WalkerGalleries
            .Where(g => g.WalkerId == walkerId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public Task<WalkerGallery?> GetByIdAsync(Guid id) =>
        dbContext.WalkerGalleries.FirstOrDefaultAsync(g => g.Id == id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.WalkerGalleries.FirstOrDefaultAsync(g => g.Id == id);
        if (existing is null) return false;
        dbContext.WalkerGalleries.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}