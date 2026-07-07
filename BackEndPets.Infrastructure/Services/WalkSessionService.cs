using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.DTOs.WalkSessions;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkSessionService(
    IWalkSessionRepository sessionRepo,
    IWalkRoutePointRepository routePointRepo,
    IPetWalkingHistoryRepository historyRepo,
    INotificationService notificationService,
    IWalkerRepository walkerRepo,
    IWalkBookingRepository bookingRepo,
    AppIdentityDbContext db) : IWalkSessionService
{
    public async Task<(WalkSessionResponse? Session, string? ErrorCode)> StartSessionAsync(
        Guid currentUserId, StartWalkSessionRequest request)
    {
        var walker = await walkerRepo.GetByUserIdAsync(currentUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var histories = new List<PetWalkingHistory>(request.PetWalkingHistoryIds.Count);
        foreach (var historyId in request.PetWalkingHistoryIds)
        {
            var history = await historyRepo.GetByIdAsync(historyId);
            if (history is null) return (null, "HISTORY_NOT_FOUND");
            if (history.WalkerId != walker.Id) return (null, "UNAUTHORIZED");
            histories.Add(history);
        }

        var session = new WalkSession
        {
            WalkerId = walker.Id,
            Status = "in_progress",
            StartedAt = DateTime.UtcNow
        };

        var created = await sessionRepo.CreateAsync(session);

        foreach (var history in histories)
        {
            history.WalkSessionId = created.Id;
            await historyRepo.UpdateAsync(history);
        }

        // Notificar a cada owner
        foreach (var history in histories)
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: history.UserId,
                Type: "walk_started",
                Title: "Walk started",
                Body: "Your walker has started the walk.",
                EntityId: created.Id,
                EntityType: "walk_session"));
        }

        var historyIds = histories.Select(h => h.Id).ToList();
        return (MapSession(created, historyIds), null);
    }

    public async Task<(WalkRoutePointResponse? Point, string? ErrorCode)> AddLocationAsync(
        Guid currentUserId, Guid sessionId, AddLocationRequest request)
    {
        var (session, errorCode) = await LoadSessionAsWalkerAsync(currentUserId, sessionId);
        if (session is null) return (null, errorCode);
        if (session.Status != "in_progress") return (null, "INVALID_STATUS_TRANSITION");

        var point = new WalkRoutePoint
        {
            WalkSessionId = sessionId,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        var created = await routePointRepo.CreateAsync(point);
        return (new WalkRoutePointResponse(
            created.Id,
            created.WalkSessionId,
            created.Latitude,
            created.Longitude,
            created.CreatedAt), null);
    }

    public async Task<(WalkSessionResponse? Session, string? ErrorCode)> PauseSessionAsync(
        Guid currentUserId, Guid sessionId)
    {
        var (session, errorCode) = await LoadSessionAsWalkerAsync(currentUserId, sessionId);
        if (session is null) return (null, errorCode);
        if (session.Status != "in_progress") return (null, "INVALID_STATUS_TRANSITION");

        session.Status = "paused";
        await sessionRepo.UpdateAsync(session);
        return (MapSession(session, []), null);
    }

    public async Task<(WalkSessionResponse? Session, string? ErrorCode)> ResumeSessionAsync(
        Guid currentUserId, Guid sessionId)
    {
        var (session, errorCode) = await LoadSessionAsWalkerAsync(currentUserId, sessionId);
        if (session is null) return (null, errorCode);
        if (session.Status != "paused") return (null, "INVALID_STATUS_TRANSITION");

        session.Status = "in_progress";
        await sessionRepo.UpdateAsync(session);
        return (MapSession(session, []), null);
    }

    public async Task<(WalkSessionResponse? Session, string? ErrorCode)> CompleteSessionAsync(
        Guid currentUserId, Guid sessionId)
    {
        var (session, errorCode) = await LoadSessionAsWalkerAsync(currentUserId, sessionId);
        if (session is null) return (null, errorCode);
        if (session.Status is not ("in_progress" or "paused")) return (null, "INVALID_STATUS_TRANSITION");

        session.Status = "completed";
        session.EndedAt = DateTime.UtcNow;
        await sessionRepo.UpdateAsync(session);

        // Notificar a cada owner del paseo
        var histories = await historyRepo.GetBySessionIdAsync(sessionId);
        foreach (var history in histories)
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: history.UserId,
                Type: "walk_finished",
                Title: "Walk finished",
                Body: "The walk has been completed.",
                EntityId: session.Id,
                EntityType: "walk_session"));
        }

        return (MapSession(session, histories.Select(h => h.Id).ToList()), null);
    }

    public async Task<(IReadOnlyCollection<WalkRoutePointResponse>? Points, string? ErrorCode)> GetRouteAsync(
        Guid currentUserId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");

        var walker = await walkerRepo.GetByUserIdAsync(currentUserId);
        var isWalker = walker is not null && walker.Id == session.WalkerId;

        if (!isWalker)
        {
            var histories = await historyRepo.GetBySessionIdAsync(sessionId);
            var isOwner = histories.Any(h => h.UserId == currentUserId);

            if (!isOwner && session.BookingId is not null)
            {
                var booking = await bookingRepo.GetByIdAsync(session.BookingId.Value);
                if (booking?.ClientUserId == currentUserId)
                    isOwner = true;
            }

            if (!isOwner) return (null, "UNAUTHORIZED");
        }

        var points = await routePointRepo.GetBySessionIdAsync(sessionId);
        var responses = points
            .Select(p => new WalkRoutePointResponse(p.Id, p.WalkSessionId, p.Latitude, p.Longitude, p.CreatedAt))
            .ToList();

        return (responses, null);
    }

    private async Task<(WalkSession? Session, string? ErrorCode)> LoadSessionAsWalkerAsync(
        Guid currentUserId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");

        var walker = await walkerRepo.GetByUserIdAsync(currentUserId);
        if (walker is null || walker.Id != session.WalkerId)
            return (null, "UNAUTHORIZED");

        return (session, null);
    }

    private static WalkSessionResponse MapSession(WalkSession s, IReadOnlyList<Guid> historyIds) =>
        new(s.Id, s.WalkerId, historyIds, s.Status, s.StartedAt, s.EndedAt);

    // ── Booking-based lifecycle ───────────────────────────────────────────────

    public async Task<(BookingSessionResponse? Session, string? ErrorCode)> StartFromBookingAsync(
        Guid walkerUserId, Guid bookingId)
    {
        var walker = await walkerRepo.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var booking = await bookingRepo.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.WalkerId != walker.Id) return (null, "UNAUTHORIZED");

        // Idempotent re-entry: walk already started for this booking → return existing session.
        if (booking.Status == "in_progress" && booking.WalkSessionId.HasValue)
        {
            var resumeSession = await sessionRepo.GetByIdAsync(booking.WalkSessionId.Value);
            if (resumeSession is not null) return (MapBookingSession(resumeSession), null);
        }

        if (booking.Status != "accepted") return (null, "BOOKING_NOT_ACCEPTED");

        var existing = await sessionRepo.GetActiveByWalkerIdAsync(walker.Id);
        if (existing is not null) return (null, "WALKER_HAS_ACTIVE_SESSION");

        var session = new WalkSession
        {
            WalkerId  = walker.Id,
            BookingId = bookingId,
            Status    = "in_progress",
            StartedAt = DateTime.UtcNow
        };

        var created = await sessionRepo.StartFromBookingAsync(session, booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "walk_started",
            Title: "Walk started",
            Body: "Your walker has started the walk.",
            EntityId: created.Id,
            EntityType: "walk_session"));

        return (MapBookingSession(created), null);
    }

    public async Task<(BookingSessionResponse? Session, string? ErrorCode)> FinishAsync(
        Guid walkerUserId, Guid sessionId, FinishSessionRequest request)
    {
        var walker = await walkerRepo.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");
        if (session.WalkerId != walker.Id) return (null, "UNAUTHORIZED");
        if (session.Status != "in_progress") return (null, "SESSION_NOT_ACTIVE");
        if (session.BookingId is null) return (null, "SESSION_NOT_BOOKING_BASED");

        var booking = await bookingRepo.GetByIdAsync(session.BookingId.Value);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");

        session.Status               = "completed";
        session.EndedAt              = DateTime.UtcNow;
        session.TotalDistanceMeters  = request.TotalDistanceMeters;
        session.TotalDurationSeconds = request.TotalDurationSeconds;

        await sessionRepo.FinishFromBookingAsync(session, booking);

        await historyRepo.CreateAsync(new PetWalkingHistory
        {
            UserId        = booking.ClientUserId,
            PetId         = booking.PetId,
            WalkerId      = booking.WalkerId,
            WalkSessionId = session.Id,
            Cost          = booking.TotalPrice,
            StartTime     = session.StartedAt,
            EndTime       = session.EndedAt
        });

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "walk_finished",
            Title: "Walk finished",
            Body: "The walk has been completed.",
            EntityId: session.Id,
            EntityType: "walk_session"));

        return (MapBookingSession(session), null);
    }

    public async Task<(BookingSessionResponse? Session, string? ErrorCode)> GetSessionDetailAsync(
        Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");
        return (MapBookingSession(session), null);
    }

    public async Task<(BookingSessionResponse? Session, string? ErrorCode)> GetActiveForWalkerAsync(
        Guid walkerUserId)
    {
        var walker = await walkerRepo.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var session = await sessionRepo.GetActiveByWalkerIdAsync(walker.Id);
        if (session is null) return (null, "NO_ACTIVE_SESSION");

        return (MapBookingSession(session), null);
    }

    private static BookingSessionResponse MapBookingSession(WalkSession s) => new(
        s.Id,
        s.WalkerId ?? Guid.Empty,
        s.BookingId ?? Guid.Empty,
        s.Status,
        s.StartedAt,
        s.EndedAt,
        s.TotalDistanceMeters,
        s.TotalDurationSeconds);

    // ── Walk session media ────────────────────────────────────────────────────

    public async Task<(WalkSessionMediaResponse? Media, string? ErrorCode)> AddMediaAsync(
        Guid walkerUserId, Guid sessionId, AddWalkMediaRequest request)
    {
        var (session, errorCode) = await LoadSessionAsWalkerAsync(walkerUserId, sessionId);
        if (session is null) return (null, errorCode);

        var media = new WalkSessionMedia
        {
            WalkSessionId = sessionId,
            Url           = request.Url,
            Type          = request.Type,
            UploadedAt    = DateTime.UtcNow
        };

        db.WalkSessionMediaItems.Add(media);
        await db.SaveChangesAsync();

        var (_, mediaOwnerIds) = await GetParticipantsAsync(session);
        foreach (var ownerId in mediaOwnerIds)
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: ownerId,
                Type: "walk_media_uploaded",
                Title: request.Type == "video" ? "New walk video" : "New walk photo",
                Body: request.Type == "video"
                    ? "Your walker uploaded a new video from the walk."
                    : "Your walker uploaded a new photo from the walk.",
                EntityId: session.Id,
                EntityType: "walk_session"));
        }

        return (MapMedia(media), null);
    }

    public async Task<(IReadOnlyCollection<WalkSessionMediaResponse>? Media, string? ErrorCode)> GetMediaAsync(
        Guid currentUserId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");

        // Allow access for the assigned walker or the booking's pet owner.
        var walker  = await walkerRepo.GetByUserIdAsync(currentUserId);
        var isWalker = walker is not null && walker.Id == session.WalkerId;

        if (!isWalker)
        {
            var isOwner = false;

            if (session.BookingId is not null)
            {
                var booking = await bookingRepo.GetByIdAsync(session.BookingId.Value);
                isOwner = booking?.ClientUserId == currentUserId;
            }

            if (!isOwner)
            {
                var histories = await historyRepo.GetBySessionIdAsync(sessionId);
                isOwner = histories.Any(h => h.UserId == currentUserId);
            }

            if (!isOwner) return (null, "UNAUTHORIZED");
        }

        var items = await db.WalkSessionMediaItems
            .Where(m => m.WalkSessionId == sessionId)
            .OrderBy(m => m.UploadedAt)
            .ToListAsync();

        return (items.Select(MapMedia).ToList(), null);
    }

    private static WalkSessionMediaResponse MapMedia(WalkSessionMedia m) => new(
        m.Id,
        m.WalkSessionId,
        m.Url,
        m.Type,
        m.UploadedAt);

    // ── Walk chat ──────────────────────────────────────────────────────────────

    public async Task<(ChatMessageResponse? Message, string? ErrorCode)> SendChatMessageAsync(
        Guid currentUserId, Guid sessionId, SendChatMessageRequest request)
    {
        var (session, errorCode) = await LoadSessionAsParticipantAsync(currentUserId, sessionId);
        if (session is null) return (null, errorCode);

        var message = new ChatMessage
        {
            WalkSessionId = sessionId,
            SenderUserId  = currentUserId,
            Text          = request.Text,
            CreatedAt     = DateTime.UtcNow
        };

        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        var (walkerUserId, chatOwnerIds) = await GetParticipantsAsync(session);
        var recipients = new HashSet<Guid>(chatOwnerIds);
        if (walkerUserId is Guid w) recipients.Add(w);
        recipients.Remove(currentUserId);

        var preview = request.Text.Length > 80 ? request.Text[..80] + "…" : request.Text;
        foreach (var recipientId in recipients)
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: recipientId,
                Type: "chat_message",
                Title: "New message",
                Body: preview,
                EntityId: session.Id,
                EntityType: "walk_session"));
        }

        return (MapChatMessage(message), null);
    }

    private async Task<(Guid? WalkerUserId, IReadOnlyCollection<Guid> OwnerUserIds)> GetParticipantsAsync(
        WalkSession session)
    {
        Guid? walkerUserId = null;
        if (session.WalkerId is Guid walkerId)
        {
            var walker = await walkerRepo.GetByIdAsync(walkerId);
            walkerUserId = walker?.UserId;
        }

        var ownerIds = new HashSet<Guid>();
        if (session.BookingId is Guid bookingId)
        {
            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking is not null) ownerIds.Add(booking.ClientUserId);
        }

        var histories = await historyRepo.GetBySessionIdAsync(session.Id);
        foreach (var h in histories) ownerIds.Add(h.UserId);

        return (walkerUserId, ownerIds);
    }

    public async Task<(IReadOnlyCollection<ChatMessageResponse>? Messages, string? ErrorCode)> GetChatMessagesAsync(
        Guid currentUserId, Guid sessionId)
    {
        var (session, errorCode) = await LoadSessionAsParticipantAsync(currentUserId, sessionId);
        if (session is null) return (null, errorCode);

        var messages = await db.ChatMessages
            .Where(m => m.WalkSessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return (messages.Select(MapChatMessage).ToList(), null);
    }

    private async Task<(WalkSession? Session, string? ErrorCode)> LoadSessionAsParticipantAsync(
        Guid currentUserId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");

        // Allow access for the assigned walker or the booking's pet owner.
        var walker  = await walkerRepo.GetByUserIdAsync(currentUserId);
        var isWalker = walker is not null && walker.Id == session.WalkerId;

        if (!isWalker)
        {
            var isOwner = false;

            if (session.BookingId is not null)
            {
                var booking = await bookingRepo.GetByIdAsync(session.BookingId.Value);
                isOwner = booking?.ClientUserId == currentUserId;
            }

            if (!isOwner)
            {
                var histories = await historyRepo.GetBySessionIdAsync(sessionId);
                isOwner = histories.Any(h => h.UserId == currentUserId);
            }

            if (!isOwner) return (null, "UNAUTHORIZED");
        }

        return (session, null);
    }

    private static ChatMessageResponse MapChatMessage(ChatMessage m) => new(
        m.Id,
        m.WalkSessionId,
        m.SenderUserId,
        m.Text,
        m.CreatedAt);
}