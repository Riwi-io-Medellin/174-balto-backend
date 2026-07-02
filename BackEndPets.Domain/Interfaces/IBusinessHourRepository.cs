using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IBusinessHourRepository
{
    Task<IReadOnlyCollection<BusinessHour>> GetByBusinessIdAsync(Guid businessId);
    Task<IReadOnlyCollection<BusinessHour>> ReplaceAsync(Guid businessId, IEnumerable<BusinessHour> slots);
}