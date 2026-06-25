using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkerDocumentRepository(AppIdentityDbContext dbContext) : IWalkerDocumentRepository
{
    public async Task<WalkerDocument> CreateAsync(WalkerDocument document)
    {
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        dbContext.WalkerDocuments.Add(document);
        await dbContext.SaveChangesAsync();
        return document;
    }

    public async Task<IReadOnlyCollection<WalkerDocument>> GetByWalkerIdAsync(Guid walkerId) =>
        await dbContext.WalkerDocuments
            .Where(d => d.WalkerId == walkerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

    public Task<WalkerDocument?> GetByIdAsync(Guid id) =>
        dbContext.WalkerDocuments.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.WalkerDocuments.FirstOrDefaultAsync(d => d.Id == id);
        if (existing is null) return false;
        dbContext.WalkerDocuments.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}