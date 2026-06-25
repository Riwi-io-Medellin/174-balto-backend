using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkRoutePointRepository(AppIdentityDbContext dbContext) : IWalkRoutePointRepository
{
    public async Task<WalkRoutePoint> CreateAsync(WalkRoutePoint point)
    {
        point.Id = Guid.NewGuid();
        point.CreatedAt = DateTime.UtcNow;
        dbContext.WalkRoutePoints.Add(point);
        await dbContext.SaveChangesAsync();
        return point;
    }

    public async Task<IReadOnlyCollection<WalkRoutePoint>> GetBySessionIdAsync(Guid sessionId) =>
        await dbContext.WalkRoutePoints
            .Where(p => p.WalkSessionId == sessionId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
}
