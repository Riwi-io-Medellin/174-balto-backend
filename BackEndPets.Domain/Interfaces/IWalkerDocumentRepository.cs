using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkerDocumentRepository
{
    Task<WalkerDocument> CreateAsync(WalkerDocument document);
    Task<IReadOnlyCollection<WalkerDocument>> GetByWalkerIdAsync(Guid walkerId);
    Task<WalkerDocument?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}