using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkerAvailabilityExceptionRepository(AppIdentityDbContext dbContext)
    : IWalkerAvailabilityExceptionRepository
{
    public async Task<IReadOnlyCollection<WalkerAvailabilityException>> GetByWalkerIdAsync(Guid walkerId) =>
        await dbContext.WalkerAvailabilityExceptions
            .Where(e => e.WalkerId == walkerId)
            .OrderBy(e => e.Date)
            .ToListAsync();

    public async Task<IReadOnlyCollection<WalkerAvailabilityException>> ReplaceAsync(
        Guid walkerId, IEnumerable<WalkerAvailabilityException> exceptions)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.WalkerAvailabilityExceptions
            .Where(e => e.WalkerId == walkerId)
            .ToListAsync();

        dbContext.WalkerAvailabilityExceptions.RemoveRange(existing);

        var newExceptions = exceptions.Select(e =>
        {
            e.Id        = Guid.NewGuid();
            e.WalkerId  = walkerId;
            e.CreatedAt = DateTime.UtcNow;
            return e;
        }).ToList();

        dbContext.WalkerAvailabilityExceptions.AddRange(newExceptions);
        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return newExceptions;
    }
}
