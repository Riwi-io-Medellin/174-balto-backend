using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeServiceSessionRepository
{
    Task<HomeServiceSession?> GetByIdAsync(Guid sessionId);
    Task<HomeServiceSession> CreateAsync(HomeServiceSession session);
    Task UpdateAsync(HomeServiceSession session);
    Task<HomeServiceSession?> GetActiveByProviderIdAsync(Guid providerId);
    Task<HomeServiceSession> StartFromBookingAsync(HomeServiceSession session, HomeServiceBooking booking);
    Task FinishFromBookingAsync(HomeServiceSession session, HomeServiceBooking booking);
}
