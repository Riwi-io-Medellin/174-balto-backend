using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeServiceBookingRepository(AppIdentityDbContext dbContext) : IHomeServiceBookingRepository
{
    public async Task<HomeServiceBooking> CreateAsync(HomeServiceBooking booking)
    {
        booking.Id        = Guid.NewGuid();
        booking.CreatedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        dbContext.HomeServiceBookings.Add(booking);
        await dbContext.SaveChangesAsync();
        return booking;
    }

    public Task<HomeServiceBooking?> GetByIdAsync(Guid id) =>
        dbContext.HomeServiceBookings.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyCollection<HomeServiceBooking>> GetByClientUserIdAsync(
        Guid clientUserId, string? status = null)
    {
        var query = dbContext.HomeServiceBookings.Where(b => b.ClientUserId == clientUserId);
        if (status is not null)
            query = query.Where(b => b.Status == status);
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<IReadOnlyCollection<HomeServiceBooking>> GetByProviderIdAsync(
        Guid providerId, string? status = null)
    {
        var query = dbContext.HomeServiceBookings.Where(b => b.ProviderId == providerId);
        if (status is not null)
            query = query.Where(b => b.Status == status);
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<HomeServiceBooking> UpdateAsync(HomeServiceBooking booking)
    {
        booking.UpdatedAt = DateTime.UtcNow;
        dbContext.HomeServiceBookings.Update(booking);
        await dbContext.SaveChangesAsync();
        return booking;
    }

    public async Task<(HomeServiceBooking Booking, HomeServiceSession Session)> AcceptWithSessionAsync(
        HomeServiceBooking booking, HomeServiceSession session)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        session.Id = Guid.NewGuid();
        dbContext.HomeServiceSessions.Add(session);

        booking.Status               = "accepted";
        booking.HomeServiceSessionId = session.Id;
        booking.AcceptedAt           = DateTime.UtcNow;
        booking.UpdatedAt            = DateTime.UtcNow;
        dbContext.HomeServiceBookings.Update(booking);

        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return (booking, session);
    }
}
