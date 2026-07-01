using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderGalleryRepository(AppIdentityDbContext dbContext) : IHomeProviderGalleryRepository
{
    public async Task<HomeProviderGallery> CreateAsync(HomeProviderGallery photo)
    {
        photo.Id = Guid.NewGuid();
        photo.CreatedAt = DateTime.UtcNow;
        dbContext.HomeProviderGalleries.Add(photo);
        await dbContext.SaveChangesAsync();
        return photo;
    }

    public async Task<IReadOnlyCollection<HomeProviderGallery>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderGalleries
            .Where(g => g.ProviderId == providerId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public Task<HomeProviderGallery?> GetByIdAsync(Guid id) =>
        dbContext.HomeProviderGalleries.FirstOrDefaultAsync(g => g.Id == id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.HomeProviderGalleries.FirstOrDefaultAsync(g => g.Id == id);
        if (existing is null) return false;
        dbContext.HomeProviderGalleries.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
