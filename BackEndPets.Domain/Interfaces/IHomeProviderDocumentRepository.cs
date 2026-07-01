using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderDocumentRepository
{
    Task<HomeProviderDocument> CreateAsync(HomeProviderDocument document);
    Task<IReadOnlyCollection<HomeProviderDocument>> GetByProviderIdAsync(Guid providerId);
    Task<HomeProviderDocument?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
