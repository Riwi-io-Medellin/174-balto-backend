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

    public Task<WalkSession?> GetActiveByWalkerIdAsync(Guid walkerId) =>
        dbContext.WalkSessions.FirstOrDefaultAsync(s =>
            s.WalkerId == walkerId && s.Status == "in_progress");

    public async Task<WalkSession> StartFromBookingAsync(WalkSession session, WalkBooking booking)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        session.Id = Guid.NewGuid();
        dbContext.WalkSessions.Add(session);
        await dbContext.SaveChangesAsync(); // INSERT walk_sessions before booking FK update

        booking.Status        = "in_progress";
        booking.WalkSessionId = session.Id;
        booking.UpdatedAt     = DateTime.UtcNow;
        dbContext.WalkBookings.Update(booking);
        await dbContext.SaveChangesAsync(); // UPDATE walk_bookings (FK row now exists)

        await tx.CommitAsync();
        return session;
    }

    public async Task FinishFromBookingAsync(WalkSession session, WalkBooking booking)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        dbContext.WalkSessions.Update(session);

        booking.Status    = "completed";
        booking.UpdatedAt = DateTime.UtcNow;
        dbContext.WalkBookings.Update(booking);

        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
