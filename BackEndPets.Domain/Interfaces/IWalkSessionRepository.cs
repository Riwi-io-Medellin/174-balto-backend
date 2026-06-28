using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkSessionRepository
{
    Task<WalkSession?> GetByIdAsync(Guid sessionId);
    Task<WalkSession> CreateAsync(WalkSession session);
    Task UpdateAsync(WalkSession session);
    Task<WalkSession?> GetActiveByWalkerIdAsync(Guid walkerId);
    Task<WalkSession> StartFromBookingAsync(WalkSession session, WalkBooking booking);
    Task FinishFromBookingAsync(WalkSession session, WalkBooking booking);
}
