using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class WalkBookingRepository(AppIdentityDbContext dbContext) : IWalkBookingRepository
{
    public async Task<WalkBooking> CreateAsync(WalkBooking booking)
    {
        booking.Id        = Guid.NewGuid();
        booking.CreatedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        dbContext.WalkBookings.Add(booking);
        await dbContext.SaveChangesAsync();
        return booking;
    }

    public Task<WalkBooking?> GetByIdAsync(Guid id) =>
        dbContext.WalkBookings.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyCollection<WalkBooking>> GetByClientUserIdAsync(
        Guid clientUserId, string? status = null)
    {
        var query = dbContext.WalkBookings.Where(b => b.ClientUserId == clientUserId);
        if (status is not null)
            query = query.Where(b => b.Status == status);
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<IReadOnlyCollection<WalkBooking>> GetByWalkerIdAsync(
        Guid walkerId, string? status = null)
    {
        var query = dbContext.WalkBookings.Where(b => b.WalkerId == walkerId);
        if (status is not null)
            query = query.Where(b => b.Status == status);
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<WalkBooking> UpdateAsync(WalkBooking booking)
    {
        booking.UpdatedAt = DateTime.UtcNow;
        dbContext.WalkBookings.Update(booking);
        await dbContext.SaveChangesAsync();
        return booking;
    }

    public async Task<(WalkBooking Booking, WalkSession Session)> AcceptWithSessionAsync(
        WalkBooking booking, WalkSession session)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        session.Id = Guid.NewGuid();
        dbContext.WalkSessions.Add(session);

        booking.Status        = "accepted";
        booking.WalkSessionId = session.Id;
        booking.UpdatedAt     = DateTime.UtcNow;
        dbContext.WalkBookings.Update(booking);

        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return (booking, session);
    }
}
