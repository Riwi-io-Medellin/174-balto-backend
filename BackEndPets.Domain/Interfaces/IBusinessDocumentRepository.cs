using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IBusinessDocumentRepository
{
    Task<BusinessDocument> CreateAsync(BusinessDocument document);
    Task<IReadOnlyCollection<BusinessDocument>> GetByBusinessIdAsync(Guid businessId);
    Task<BusinessDocument?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}