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
    
    public async Task<IReadOnlyCollection<Business>> GetAllAsync(string? type = null, string? location = null)
    {
        var query = dbContext.Businesses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(b => b.Type == type);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(b => b.Location != null &&
                                     b.Location.ToLower().Contains(location.ToLower()));

        return await query.OrderBy(b => b.CreatedAt).ToListAsync();
    }

    public Task<Business?> GetByIdAsync(Guid id) =>
        dbContext.Businesses.FirstOrDefaultAsync(b => b.Id == id);
    
    public async Task<Business> UpdateAsync(Business business)
    {
        dbContext.Businesses.Update(business);
        await dbContext.SaveChangesAsync();
        return business;
    }
}
