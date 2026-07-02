using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceSessionAuthorizationService(
    IHomeServiceSessionRepository sessionRepo,
    IHomeServiceProviderRepository providerRepo,
    IHomeServiceBookingRepository bookingRepo) : IHomeServiceSessionAuthorizationService
{
    public async Task<HomeServiceGroupAccessResult> CanJoinSessionGroupAsync(Guid userId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null)
            return new HomeServiceGroupAccessResult(false, "Session not found.");

        var provider = await providerRepo.GetByUserIdAsync(userId);
        var isProvider = provider is not null && provider.Id == session.ProviderId;
        if (isProvider)
            return new HomeServiceGroupAccessResult(true, null);

        if (session.BookingId is not null)
        {
            var booking = await bookingRepo.GetByIdAsync(session.BookingId.Value);
            if (booking?.ClientUserId == userId)
                return new HomeServiceGroupAccessResult(true, null);
        }

        return new HomeServiceGroupAccessResult(false, "Unauthorized.");
    }
}
