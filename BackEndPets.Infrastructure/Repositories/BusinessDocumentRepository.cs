using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class BusinessDocumentRepository(AppIdentityDbContext dbContext) : IBusinessDocumentRepository
{
    public async Task<BusinessDocument> CreateAsync(BusinessDocument document)
    {
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        dbContext.BusinessDocuments.Add(document);
        await dbContext.SaveChangesAsync();
        return document;
    }

    public async Task<IReadOnlyCollection<BusinessDocument>> GetByBusinessIdAsync(Guid businessId) =>
        await dbContext.BusinessDocuments
            .Where(d => d.BusinessId == businessId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

    public Task<BusinessDocument?> GetByIdAsync(Guid id) =>
        dbContext.BusinessDocuments.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.BusinessDocuments.FirstOrDefaultAsync(d => d.Id == id);
        if (existing is null) return false;
        dbContext.BusinessDocuments.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}