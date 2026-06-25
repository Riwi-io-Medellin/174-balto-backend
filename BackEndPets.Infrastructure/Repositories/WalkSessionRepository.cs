using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkSessionRepository(AppIdentityDbContext dbContext) : IWalkSessionRepository
{
    public Task<WalkSession?> GetByIdAsync(Guid sessionId) =>
        dbContext.WalkSessions.FirstOrDefaultAsync(s => s.Id == sessionId);

    public async Task<WalkSession> CreateAsync(WalkSession session)
    {
        session.Id = Guid.NewGuid();
        dbContext.WalkSessions.Add(session);
        await dbContext.SaveChangesAsync();
        return session;
    }

    public async Task UpdateAsync(WalkSession session)
    {
        dbContext.WalkSessions.Update(session);
        await dbContext.SaveChangesAsync();
    }
}
