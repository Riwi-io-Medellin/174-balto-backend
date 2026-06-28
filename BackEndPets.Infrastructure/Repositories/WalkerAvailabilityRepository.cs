using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkerAvailabilityRepository(AppIdentityDbContext dbContext) : IWalkerAvailabilityRepository
{
    public async Task<IReadOnlyCollection<WalkerAvailability>> GetByWalkerIdAsync(Guid walkerId) =>
        await dbContext.WalkerAvailabilities
            .Where(a => a.WalkerId == walkerId)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

    public async Task<IReadOnlyCollection<WalkerAvailability>> ReplaceAsync(
        Guid walkerId, IEnumerable<WalkerAvailability> slots)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.WalkerAvailabilities
            .Where(a => a.WalkerId == walkerId)
            .ToListAsync();

        dbContext.WalkerAvailabilities.RemoveRange(existing);

        var newSlots = slots.Select(s =>
        {
            s.Id        = Guid.NewGuid();
            s.WalkerId  = walkerId;
            s.CreatedAt = DateTime.UtcNow;
            return s;
        }).ToList();

        dbContext.WalkerAvailabilities.AddRange(newSlots);
        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return newSlots;
    }
}
