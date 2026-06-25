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
}
