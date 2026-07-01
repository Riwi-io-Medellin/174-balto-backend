using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderAvailabilityRepository(AppIdentityDbContext dbContext)
    : IHomeProviderAvailabilityRepository
{
    public async Task<IReadOnlyCollection<HomeProviderAvailability>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderAvailabilities
            .Where(a => a.ProviderId == providerId)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

    public async Task<IReadOnlyCollection<HomeProviderAvailability>> ReplaceAsync(
        Guid providerId, IEnumerable<HomeProviderAvailability> slots)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.HomeProviderAvailabilities
            .Where(a => a.ProviderId == providerId)
            .ToListAsync();

        dbContext.HomeProviderAvailabilities.RemoveRange(existing);

        var newSlots = slots.Select(s =>
        {
            s.Id         = Guid.NewGuid();
            s.ProviderId = providerId;
            s.CreatedAt  = DateTime.UtcNow;
            return s;
        }).ToList();

        dbContext.HomeProviderAvailabilities.AddRange(newSlots);
        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return newSlots;
    }
}
