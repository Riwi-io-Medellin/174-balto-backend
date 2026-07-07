using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceSessionService(
    IHomeServiceSessionRepository sessionRepo,
    IHomeServiceSessionEventRepository eventRepo,
    IHomeServiceProviderRepository providerRepo,
    IHomeServiceBookingRepository bookingRepo,
    INotificationService notificationService) : IHomeServiceSessionService
{
    public async Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> StartFromBookingAsync(
        Guid providerUserId, Guid bookingId)
    {
        var provider = await providerRepo.GetByUserIdAsync(providerUserId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var booking = await bookingRepo.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.ProviderId != provider.Id) return (null, "UNAUTHORIZED");

        // Idempotent re-entry: session already started for this booking → return existing session.
        if (booking.Status == "in_progress" && booking.HomeServiceSessionId.HasValue)
        {
            var resumeSession = await sessionRepo.GetByIdAsync(booking.HomeServiceSessionId.Value);
            if (resumeSession is not null) return (MapSession(resumeSession), null);
        }

        if (booking.Status != "accepted") return (null, "BOOKING_NOT_ACCEPTED");

        var existing = await sessionRepo.GetActiveByProviderIdAsync(provider.Id);
        if (existing is not null) return (null, "PROVIDER_HAS_ACTIVE_SESSION");

        var session = new HomeServiceSession
        {
            ProviderId = provider.Id,
            BookingId  = bookingId,
            Status     = "in_progress",
            StartedAt  = DateTime.UtcNow
        };

        var created = await sessionRepo.StartFromBookingAsync(session, booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "system",
            Title: "Service started",
            Body: "Your provider has started the service.",
            EntityId: created.Id,
            EntityType: "home_service_session"));

        return (MapSession(created), null);
    }

    public async Task<(HomeServiceSessionEventResponse? Event, string? ErrorCode)> AddEventAsync(
        Guid providerUserId, Guid sessionId, AddHomeServiceSessionEventRequest request)
    {
        var (session, errorCode) = await LoadSessionAsProviderAsync(providerUserId, sessionId);
        if (session is null) return (null, errorCode);
        if (session.Status != "in_progress") return (null, "INVALID_STATUS_TRANSITION");

        var sessionEvent = new HomeServiceSessionEvent
        {
            SessionId   = sessionId,
            EventType   = request.EventType.Trim(),
            Description = request.Description?.Trim(),
            Latitude    = request.Latitude,
            Longitude   = request.Longitude
        };

        var created = await eventRepo.CreateAsync(sessionEvent);
        return (MapEvent(created), null);
    }

    public async Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> FinishAsync(
        Guid providerUserId, Guid sessionId, FinishHomeServiceSessionRequest request)
    {
        var provider = await providerRepo.GetByUserIdAsync(providerUserId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");
        if (session.ProviderId != provider.Id) return (null, "UNAUTHORIZED");
        if (session.Status != "in_progress") return (null, "SESSION_NOT_ACTIVE");
        if (session.BookingId is null) return (null, "SESSION_NOT_BOOKING_BASED");

        var booking = await bookingRepo.GetByIdAsync(session.BookingId.Value);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");

        session.Status               = "completed";
        session.EndedAt              = DateTime.UtcNow;
        session.TotalDistanceMeters  = request.TotalDistanceMeters;
        session.TotalDurationSeconds = request.TotalDurationSeconds;

        await sessionRepo.FinishFromBookingAsync(session, booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "system",
            Title: "Service finished",
            Body: "The service has been completed.",
            EntityId: session.Id,
            EntityType: "home_service_session"));

        return (MapSession(session), null);
    }

    public async Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> GetSessionDetailAsync(Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");
        return (MapSession(session), null);
    }

    public async Task<(IReadOnlyCollection<HomeServiceSessionEventResponse>? Events, string? ErrorCode)> GetEventsAsync(
        Guid currentUserId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");

        var provider = await providerRepo.GetByUserIdAsync(currentUserId);
        var isProvider = provider is not null && provider.Id == session.ProviderId;

        if (!isProvider)
        {
            var isClient = session.BookingId is not null &&
                (await bookingRepo.GetByIdAsync(session.BookingId.Value))?.ClientUserId == currentUserId;
            if (!isClient) return (null, "UNAUTHORIZED");
        }

        var events = await eventRepo.GetBySessionIdAsync(sessionId);
        return (events.Select(MapEvent).ToList(), null);
    }

    public async Task<(HomeServiceSessionResponse? Session, string? ErrorCode)> GetActiveForProviderAsync(
        Guid providerUserId)
    {
        var provider = await providerRepo.GetByUserIdAsync(providerUserId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var session = await sessionRepo.GetActiveByProviderIdAsync(provider.Id);
        if (session is null) return (null, "NO_ACTIVE_SESSION");

        return (MapSession(session), null);
    }

    private async Task<(HomeServiceSession? Session, string? ErrorCode)> LoadSessionAsProviderAsync(
        Guid providerUserId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");

        var provider = await providerRepo.GetByUserIdAsync(providerUserId);
        if (provider is null || provider.Id != session.ProviderId)
            return (null, "UNAUTHORIZED");

        return (session, null);
    }

    private static HomeServiceSessionResponse MapSession(HomeServiceSession s) => new(
        s.Id,
        s.ProviderId ?? Guid.Empty,
        s.BookingId ?? Guid.Empty,
        s.Status,
        s.StartedAt,
        s.EndedAt,
        s.TotalDistanceMeters,
        s.TotalDurationSeconds);

    private static HomeServiceSessionEventResponse MapEvent(HomeServiceSessionEvent e) => new(
        e.Id, e.SessionId, e.EventType, e.Description, e.Latitude, e.Longitude, e.CreatedAt);
}
