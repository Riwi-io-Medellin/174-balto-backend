using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeServiceSessionEventRepository(AppIdentityDbContext dbContext)
    : IHomeServiceSessionEventRepository
{
    public async Task<HomeServiceSessionEvent> CreateAsync(HomeServiceSessionEvent sessionEvent)
    {
        sessionEvent.Id = Guid.NewGuid();
        sessionEvent.CreatedAt = DateTime.UtcNow;
        dbContext.HomeServiceSessionEvents.Add(sessionEvent);
        await dbContext.SaveChangesAsync();
        return sessionEvent;
    }

    public async Task<IReadOnlyCollection<HomeServiceSessionEvent>> GetBySessionIdAsync(Guid sessionId) =>
        await dbContext.HomeServiceSessionEvents
            .Where(e => e.SessionId == sessionId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
}
