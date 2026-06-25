using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class BusinessRepository(AppIdentityDbContext dbContext) : IBusinessRepository
{
    public async Task<IReadOnlyCollection<Business>> GetByOwnerIdAsync(Guid ownerUserId) =>
        await dbContext.Businesses
            .Where(b => b.OwnerUserId == ownerUserId)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();

    public async Task<Business> CreateAsync(Business business)
    {
        business.Id = Guid.NewGuid();
        business.CreatedAt = DateTime.UtcNow;
        dbContext.Businesses.Add(business);
        await dbContext.SaveChangesAsync();
        return business;
    }
    
    public async Task<IReadOnlyCollection<Business>> GetAllAsync() =>
        await dbContext.Businesses
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();

    public Task<Business?> GetByIdAsync(Guid id) =>
        dbContext.Businesses.FirstOrDefaultAsync(b => b.Id == id);
}
