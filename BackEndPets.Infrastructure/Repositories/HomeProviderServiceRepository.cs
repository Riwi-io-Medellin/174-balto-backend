using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderServiceRepository(AppIdentityDbContext dbContext) : IHomeProviderServiceRepository
{
    public async Task<HomeProviderService> CreateAsync(HomeProviderService service)
    {
        service.Id = Guid.NewGuid();
        service.CreatedAt = DateTime.UtcNow;
        dbContext.HomeProviderServices.Add(service);
        await dbContext.SaveChangesAsync();
        return service;
    }

    public async Task<IReadOnlyCollection<HomeProviderService>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderServices
            .Where(s => s.ProviderId == providerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public Task<HomeProviderService?> GetByIdAsync(Guid id) =>
        dbContext.HomeProviderServices.FirstOrDefaultAsync(s => s.Id == id);

    public Task<bool> ExistsAsync(Guid providerId, Guid serviceTypeId) =>
        dbContext.HomeProviderServices.AnyAsync(s => s.ProviderId == providerId && s.ServiceTypeId == serviceTypeId);

    public async Task<HomeProviderService?> UpdateAsync(HomeProviderService service)
    {
        var existing = await dbContext.HomeProviderServices.FirstOrDefaultAsync(s => s.Id == service.Id);
        if (existing is null) return null;

        existing.Price = service.Price;
        existing.PriceUnit = service.PriceUnit;
        existing.Description = service.Description;
        existing.IsActive = service.IsActive;

        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.HomeProviderServices.FirstOrDefaultAsync(s => s.Id == id);
        if (existing is null) return false;
        dbContext.HomeProviderServices.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
