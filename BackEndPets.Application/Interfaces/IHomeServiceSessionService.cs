using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeServiceSessionService
{
    Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> StartFromBookingAsync(
        Guid providerUserId, Guid bookingId);

    Task<(HomeServiceSessionEventResponse? Event, string? ErrorCode)> AddEventAsync(
        Guid providerUserId, Guid sessionId, AddHomeServiceSessionEventRequest request);

    Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> FinishAsync(
        Guid providerUserId, Guid sessionId, FinishHomeServiceSessionRequest request);

    Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> GetSessionDetailAsync(Guid sessionId);

    Task<(IReadOnlyCollection<HomeServiceSessionEventResponse>? Events, string? ErrorCode)> GetEventsAsync(
        Guid currentUserId, Guid sessionId);

    Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> GetActiveForProviderAsync(Guid providerUserId);
}
