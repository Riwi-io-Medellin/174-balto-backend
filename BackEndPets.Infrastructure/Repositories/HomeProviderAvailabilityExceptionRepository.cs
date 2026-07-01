using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderAvailabilityExceptionRepository(AppIdentityDbContext dbContext)
    : IHomeProviderAvailabilityExceptionRepository
{
    public async Task<IReadOnlyCollection<HomeProviderAvailabilityException>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderAvailabilityExceptions
            .Where(e => e.ProviderId == providerId)
            .OrderBy(e => e.Date)
            .ToListAsync();

    public async Task<IReadOnlyCollection<HomeProviderAvailabilityException>> ReplaceAsync(
        Guid providerId, IEnumerable<HomeProviderAvailabilityException> exceptions)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.HomeProviderAvailabilityExceptions
            .Where(e => e.ProviderId == providerId)
            .ToListAsync();

        dbContext.HomeProviderAvailabilityExceptions.RemoveRange(existing);

        var newExceptions = exceptions.Select(e =>
        {
            e.Id         = Guid.NewGuid();
            e.ProviderId = providerId;
            e.CreatedAt  = DateTime.UtcNow;
            return e;
        }).ToList();

        dbContext.HomeProviderAvailabilityExceptions.AddRange(newExceptions);
        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return newExceptions;
    }
}
