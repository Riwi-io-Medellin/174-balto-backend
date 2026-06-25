using BackEndPets.Application.DTOs.WalkSessions;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkSessionService(
    IWalkSessionRepository sessionRepo,
    IWalkRoutePointRepository routePointRepo,
    IPetWalkingHistoryRepository historyRepo,
    IWalkerRepository walkerRepo) : IWalkSessionService
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
        return (MapSession(session, []), null);
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
}
