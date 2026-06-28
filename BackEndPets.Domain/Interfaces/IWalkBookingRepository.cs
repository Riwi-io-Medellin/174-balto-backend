using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkBookingRepository
{
    Task<WalkBooking> CreateAsync(WalkBooking booking);
    Task<WalkBooking?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<WalkBooking>> GetByClientUserIdAsync(Guid clientUserId, string? status = null);
    Task<IReadOnlyCollection<WalkBooking>> GetByWalkerIdAsync(Guid walkerId, string? status = null);
    Task<WalkBooking> UpdateAsync(WalkBooking booking);
    /// <summary>
    /// Atomically marks the booking as accepted and creates the linked WalkSession.
    /// </summary>
    Task<(WalkBooking Booking, WalkSession Session)> AcceptWithSessionAsync(
        WalkBooking booking, WalkSession session);
}
