using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderServiceAreaRepository(AppIdentityDbContext dbContext)
    : IHomeProviderServiceAreaRepository
{
    public async Task<IReadOnlyCollection<HomeProviderServiceArea>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderServiceAreas
            .Where(a => a.ProviderId == providerId)
            .OrderBy(a => a.Label)
            .ToListAsync();

    public async Task<IReadOnlyCollection<HomeProviderServiceArea>> ReplaceAsync(
        Guid providerId, IEnumerable<HomeProviderServiceArea> areas)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.HomeProviderServiceAreas
            .Where(a => a.ProviderId == providerId)
            .ToListAsync();

        dbContext.HomeProviderServiceAreas.RemoveRange(existing);

        var newAreas = areas.Select(a =>
        {
            a.Id         = Guid.NewGuid();
            a.ProviderId = providerId;
            a.CreatedAt  = DateTime.UtcNow;
            return a;
        }).ToList();

        dbContext.HomeProviderServiceAreas.AddRange(newAreas);
        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return newAreas;
    }

    public async Task<IReadOnlyCollection<HomeProviderServiceArea>> GetAllAsync() =>
        await dbContext.HomeProviderServiceAreas.ToListAsync();
}
