using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class BusinessHourRepository(AppIdentityDbContext dbContext) : IBusinessHourRepository
{
    public async Task<IReadOnlyCollection<BusinessHour>> GetByBusinessIdAsync(Guid businessId) =>
        await dbContext.BusinessHours
            .Where(a => a.BusinessId == businessId)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

    public async Task<IReadOnlyCollection<BusinessHour>> ReplaceAsync(
        Guid businessId, IEnumerable<BusinessHour> slots)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.BusinessHours
            .Where(a => a.BusinessId == businessId)
            .ToListAsync();

        dbContext.BusinessHours.RemoveRange(existing);

        var newSlots = slots.Select(s =>
        {
            s.Id         = Guid.NewGuid();
            s.BusinessId = businessId;
            s.CreatedAt  = DateTime.UtcNow;
            return s;
        }).ToList();

        dbContext.BusinessHours.AddRange(newSlots);
        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return newSlots;
    }
}