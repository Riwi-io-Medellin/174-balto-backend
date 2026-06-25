using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class PetWalkingHistoryRepository(AppIdentityDbContext dbContext) : IPetWalkingHistoryRepository
{
    public async Task<PetWalkingHistory> CreateAsync(PetWalkingHistory history)
    {
        history.Id = Guid.NewGuid();
        history.CreatedAt = DateTime.UtcNow;
        dbContext.PetWalkingHistories.Add(history);
        await dbContext.SaveChangesAsync();
        return history;
    }

    public Task<PetWalkingHistory?> GetByIdAsync(Guid id) =>
        dbContext.PetWalkingHistories.FirstOrDefaultAsync(h => h.Id == id);

    public async Task<IReadOnlyCollection<PetWalkingHistory>> GetByUserIdAsync(Guid userId) =>
        await dbContext.PetWalkingHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyCollection<PetWalkingHistory>> GetByWalkerIdAsync(Guid walkerId) =>
        await dbContext.PetWalkingHistories
            .Where(h => h.WalkerId == walkerId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
}