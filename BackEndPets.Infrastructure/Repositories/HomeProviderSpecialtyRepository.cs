using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderSpecialtyRepository(AppIdentityDbContext dbContext) : IHomeProviderSpecialtyRepository
{
    public async Task<HomeProviderSpecialty> CreateAsync(HomeProviderSpecialty specialty)
    {
        specialty.Id = Guid.NewGuid();
        specialty.CreatedAt = DateTime.UtcNow;
        dbContext.HomeProviderSpecialties.Add(specialty);
        await dbContext.SaveChangesAsync();
        return specialty;
    }

    public async Task<IReadOnlyCollection<HomeProviderSpecialty>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderSpecialties
            .Where(s => s.ProviderId == providerId)
            .OrderBy(s => s.Specialty)
            .ToListAsync();

    public Task<HomeProviderSpecialty?> GetByIdAsync(Guid id) =>
        dbContext.HomeProviderSpecialties.FirstOrDefaultAsync(s => s.Id == id);

    public Task<bool> ExistsAsync(Guid providerId, string specialty) =>
        dbContext.HomeProviderSpecialties.AnyAsync(s => s.ProviderId == providerId && s.Specialty == specialty);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.HomeProviderSpecialties.FirstOrDefaultAsync(s => s.Id == id);
        if (existing is null) return false;
        dbContext.HomeProviderSpecialties.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
