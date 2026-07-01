using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeProviderDocumentRepository(AppIdentityDbContext dbContext) : IHomeProviderDocumentRepository
{
    public async Task<HomeProviderDocument> CreateAsync(HomeProviderDocument document)
    {
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        dbContext.HomeProviderDocuments.Add(document);
        await dbContext.SaveChangesAsync();
        return document;
    }

    public async Task<IReadOnlyCollection<HomeProviderDocument>> GetByProviderIdAsync(Guid providerId) =>
        await dbContext.HomeProviderDocuments
            .Where(d => d.ProviderId == providerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

    public Task<HomeProviderDocument?> GetByIdAsync(Guid id) =>
        dbContext.HomeProviderDocuments.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.HomeProviderDocuments.FirstOrDefaultAsync(d => d.Id == id);
        if (existing is null) return false;
        dbContext.HomeProviderDocuments.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
