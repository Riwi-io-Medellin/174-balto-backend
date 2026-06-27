using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class PetHistoryRepository(AppIdentityDbContext dbContext) : IPetHistoryRepository
{
    public async Task<PetHistory> CreateAsync(PetHistory history)
    {
        history.Id = Guid.NewGuid();
        history.CreatedAt = DateTime.UtcNow;
        dbContext.PetHistories.Add(history);
        await dbContext.SaveChangesAsync();
        return history;
    }

    public async Task<IReadOnlyCollection<PetHistory>> GetByPetIdAsync(Guid petId) =>
        await dbContext.PetHistories
            .Where(h => h.PetId == petId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

    public Task<PetHistory?> GetByIdAsync(Guid id) =>
        dbContext.PetHistories.FirstOrDefaultAsync(h => h.Id == id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.PetHistories.FirstOrDefaultAsync(h => h.Id == id);
        if (existing is null) return false;
        dbContext.PetHistories.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}