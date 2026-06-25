using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkerRepository(AppIdentityDbContext dbContext) : IWalkerRepository
{
    public Task<Walker?> GetByUserIdAsync(Guid userId) =>
        dbContext.Walkers.FirstOrDefaultAsync(w => w.UserId == userId);

    public Task<bool> ExistsForUserAsync(Guid userId) =>
        dbContext.Walkers.AnyAsync(w => w.UserId == userId);

    public async Task<Walker> CreateAsync(Walker walker)
    {
        walker.Id = Guid.NewGuid();
        walker.CreatedAt = DateTime.UtcNow;
        walker.UpdatedAt = DateTime.UtcNow;
        dbContext.Walkers.Add(walker);
        await dbContext.SaveChangesAsync();
        return walker;
    }
    
    public async Task<IReadOnlyCollection<Walker>> GetAllAsync() =>
        await dbContext.Walkers
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();

    public Task<Walker?> GetByIdAsync(Guid id) =>
        dbContext.Walkers.FirstOrDefaultAsync(w => w.Id == id);
}
