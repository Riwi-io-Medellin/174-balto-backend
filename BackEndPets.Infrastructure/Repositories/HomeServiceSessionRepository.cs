using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeServiceSessionRepository(AppIdentityDbContext dbContext) : IHomeServiceSessionRepository
{
    public Task<HomeServiceSession?> GetByIdAsync(Guid sessionId) =>
        dbContext.HomeServiceSessions.FirstOrDefaultAsync(s => s.Id == sessionId);

    public async Task<HomeServiceSession> CreateAsync(HomeServiceSession session)
    {
        session.Id = Guid.NewGuid();
        dbContext.HomeServiceSessions.Add(session);
        await dbContext.SaveChangesAsync();
        return session;
    }

    public async Task UpdateAsync(HomeServiceSession session)
    {
        dbContext.HomeServiceSessions.Update(session);
        await dbContext.SaveChangesAsync();
    }

    public Task<HomeServiceSession?> GetActiveByProviderIdAsync(Guid providerId) =>
        dbContext.HomeServiceSessions.FirstOrDefaultAsync(s =>
            s.ProviderId == providerId && s.Status == "in_progress");

    public async Task<HomeServiceSession> StartFromBookingAsync(HomeServiceSession session, HomeServiceBooking booking)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        session.Id = Guid.NewGuid();
        dbContext.HomeServiceSessions.Add(session);
        await dbContext.SaveChangesAsync(); // INSERT home_service_sessions before booking FK update

        booking.Status               = "in_progress";
        booking.HomeServiceSessionId = session.Id;
        booking.StartedAt            = DateTime.UtcNow;
        booking.UpdatedAt            = DateTime.UtcNow;
        dbContext.HomeServiceBookings.Update(booking);
        await dbContext.SaveChangesAsync(); // UPDATE home_service_bookings (FK row now exists)

        await tx.CommitAsync();
        return session;
    }

    public async Task FinishFromBookingAsync(HomeServiceSession session, HomeServiceBooking booking)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        dbContext.HomeServiceSessions.Update(session);

        booking.Status      = "completed";
        booking.CompletedAt = DateTime.UtcNow;
        booking.UpdatedAt   = DateTime.UtcNow;
        dbContext.HomeServiceBookings.Update(booking);

        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
