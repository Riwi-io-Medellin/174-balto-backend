using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class BusinessServiceRepository(AppIdentityDbContext dbContext) : IBusinessServiceRepository
{
    public async Task<BusinessService> CreateAsync(BusinessService service)
    {
        service.Id = Guid.NewGuid();
        service.CreatedAt = DateTime.UtcNow;
        dbContext.BusinessServices.Add(service);
        await dbContext.SaveChangesAsync();
        return service;
    }

    public async Task<IReadOnlyCollection<BusinessService>> GetByBusinessIdAsync(Guid businessId) =>
        await dbContext.BusinessServices
            .Where(s => s.BusinessId == businessId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public Task<BusinessService?> GetByIdAsync(Guid id) =>
        dbContext.BusinessServices.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<BusinessService?> UpdateAsync(BusinessService service)
    {
        var existing = await dbContext.BusinessServices.FirstOrDefaultAsync(s => s.Id == service.Id);
        if (existing is null) return null;

        existing.ServiceType = service.ServiceType;
        existing.Description = service.Description;
        existing.Price = service.Price;
        existing.PhotoUrl = service.PhotoUrl;

        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.BusinessServices.FirstOrDefaultAsync(s => s.Id == id);
        if (existing is null) return false;
        dbContext.BusinessServices.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}