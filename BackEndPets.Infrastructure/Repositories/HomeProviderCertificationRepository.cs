using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderCertificationRepository(AppIdentityDbContext dbContext)
    : IHomeProviderCertificationRepository
{
    public async Task<HomeProviderCertification> CreateAsync(HomeProviderCertification certification)
    {
        certification.Id = Guid.NewGuid();
        certification.CreatedAt = DateTime.UtcNow;
        dbContext.HomeProviderCertifications.Add(certification);
        await dbContext.SaveChangesAsync();
        return certification;
    }

    public async Task<IReadOnlyCollection<HomeProviderCertification>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderCertifications
            .Where(c => c.ProviderId == providerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public Task<HomeProviderCertification?> GetByIdAsync(Guid id) =>
        dbContext.HomeProviderCertifications.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.HomeProviderCertifications.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return false;
        dbContext.HomeProviderCertifications.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
