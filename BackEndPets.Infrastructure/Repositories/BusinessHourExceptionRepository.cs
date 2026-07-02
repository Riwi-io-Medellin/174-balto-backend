using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class BusinessHourExceptionRepository(AppIdentityDbContext dbContext) : IBusinessHourExceptionRepository
{
    public async Task<IReadOnlyCollection<BusinessHourException>> GetByBusinessIdAsync(Guid businessId) =>
        await dbContext.BusinessHourExceptions
            .Where(e => e.BusinessId == businessId)
            .OrderBy(e => e.Date)
            .ToListAsync();

    public async Task<IReadOnlyCollection<BusinessHourException>> ReplaceAsync(
        Guid businessId, IEnumerable<BusinessHourException> exceptions)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.BusinessHourExceptions
            .Where(e => e.BusinessId == businessId)
            .ToListAsync();

        dbContext.BusinessHourExceptions.RemoveRange(existing);

        var newExceptions = exceptions.Select(e =>
        {
            e.Id         = Guid.NewGuid();
            e.BusinessId = businessId;
            e.CreatedAt  = DateTime.UtcNow;
            return e;
        }).ToList();

        dbContext.BusinessHourExceptions.AddRange(newExceptions);
        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return newExceptions;
    }
}