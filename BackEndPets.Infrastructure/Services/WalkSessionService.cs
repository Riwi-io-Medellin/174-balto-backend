using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.DTOs.WalkSessions;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkSessionService(
    IWalkSessionRepository sessionRepo,
    IWalkRoutePointRepository routePointRepo,
    IPetWalkingHistoryRepository historyRepo,
    INotificationService notificationService,
    IWalkerRepository walkerRepo) : IWalkSessionService
{
    public async Task<(WalkSessionResponse? Session, string? ErrorCode)> StartSessionAsync(
        Guid currentUserId, StartWalkSessionRequest request)
    {
        var history = await historyRepo.GetByIdAsync(request.PetWalkingHistoryId);
        if (history is null) return (null, "HISTORY_NOT_FOUND");

        var walker = await walkerRepo.GetByUserIdAsync(currentUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");
        if (walker.Id != history.WalkerId) return (null, "UNAUTHORIZED");

        var session = new WalkSession
        {
            PetWalkingHistoryId = request.PetWalkingHistoryId,
            Status = "in_progress",
            StartedAt = DateTime.UtcNow
        };

        var created = await sessionRepo.CreateAsync(session);
        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: history.UserId,
            Type: "walk_started",
            Title: "Paseo iniciado",
            Body: "El paseador ha iniciado el paseo.",
            EntityId: session.Id,
            EntityType: "walk_session"));
        return (MapSession(created), null);
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
        return (MapSession(session), null);
    }

    public async Task<(WalkSessionResponse? Session, string? ErrorCode)> ResumeSessionAsync(
        Guid currentUserId, Guid sessionId)
    {
        var (session, errorCode) = await LoadSessionAsWalkerAsync(currentUserId, sessionId);
        if (session is null) return (null, errorCode);
        if (session.Status != "paused") return (null, "INVALID_STATUS_TRANSITION");

        session.Status = "in_progress";
        await sessionRepo.UpdateAsync(session);
        return (MapSession(session), null);
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
        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: currentUserId,
            Type: "walk_finished",
            Title: "Paseo finalizado",
            Body: "El paseo ha sido completado.",
            EntityId: session.Id,
            EntityType: "walk_session"));
        return (MapSession(session), null);
    }

    public async Task<(IReadOnlyCollection<WalkRoutePointResponse>? Points, string? ErrorCode)> GetRouteAsync(
        Guid currentUserId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return (null, "SESSION_NOT_FOUND");

        var history = await historyRepo.GetByIdAsync(session.PetWalkingHistoryId);
        if (history is null) return (null, "HISTORY_NOT_FOUND");

        var walker = await walkerRepo.GetByUserIdAsync(currentUserId);
        var isWalker = walker is not null && walker.Id == history.WalkerId;
        var isOwner = history.UserId == currentUserId;

        if (!isWalker && !isOwner) return (null, "UNAUTHORIZED");

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

        var history = await historyRepo.GetByIdAsync(session.PetWalkingHistoryId);
        if (history is null) return (null, "HISTORY_NOT_FOUND");

        var walker = await walkerRepo.GetByUserIdAsync(currentUserId);
        if (walker is null || walker.Id != history.WalkerId)
            return (null, "UNAUTHORIZED");

        return (session, null);
    }

    private static WalkSessionResponse MapSession(WalkSession s) =>
        new(s.Id, s.PetWalkingHistoryId, s.Status, s.StartedAt, s.EndedAt);
}
