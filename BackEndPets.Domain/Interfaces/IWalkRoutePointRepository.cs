using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkRoutePointRepository
{
    Task<WalkRoutePoint> CreateAsync(WalkRoutePoint point);
    Task<IReadOnlyCollection<WalkRoutePoint>> GetBySessionIdAsync(Guid sessionId);
}
