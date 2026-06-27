using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IBusinessServiceRepository
{
    Task<BusinessService> CreateAsync(BusinessService service);
    Task<IReadOnlyCollection<BusinessService>> GetByBusinessIdAsync(Guid businessId);
    Task<BusinessService?> GetByIdAsync(Guid id);
    Task<BusinessService?> UpdateAsync(BusinessService service);
    Task<bool> DeleteAsync(Guid id);
}