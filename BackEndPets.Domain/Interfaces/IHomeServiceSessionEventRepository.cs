using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeServiceSessionEventRepository
{
    Task<HomeServiceSessionEvent> CreateAsync(HomeServiceSessionEvent sessionEvent);
    Task<IReadOnlyCollection<HomeServiceSessionEvent>> GetBySessionIdAsync(Guid sessionId);
}
