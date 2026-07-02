using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IBusinessHourExceptionRepository
{
    Task<IReadOnlyCollection<BusinessHourException>> GetByBusinessIdAsync(Guid businessId);
    Task<IReadOnlyCollection<BusinessHourException>> ReplaceAsync(Guid businessId, IEnumerable<BusinessHourException> exceptions);
}