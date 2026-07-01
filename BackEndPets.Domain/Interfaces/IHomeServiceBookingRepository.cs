using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeServiceBookingRepository
{
    Task<HomeServiceBooking> CreateAsync(HomeServiceBooking booking);
    Task<HomeServiceBooking?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<HomeServiceBooking>> GetByClientUserIdAsync(Guid clientUserId, string? status = null);
    Task<IReadOnlyCollection<HomeServiceBooking>> GetByProviderIdAsync(Guid providerId, string? status = null);
    Task<HomeServiceBooking> UpdateAsync(HomeServiceBooking booking);
    /// <summary>
    /// Atomically marks the booking as accepted and creates the linked HomeServiceSession.
    /// </summary>
    Task<(HomeServiceBooking Booking, HomeServiceSession Session)> AcceptWithSessionAsync(
        HomeServiceBooking booking, HomeServiceSession session);
}
