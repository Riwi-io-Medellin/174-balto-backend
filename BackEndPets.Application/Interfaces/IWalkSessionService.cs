using BackEndPets.Application.DTOs.WalkSessions;

namespace BackEndPets.Application.Interfaces;

public interface IWalkSessionService
{
    Task<(WalkSessionResponse? Session, string? ErrorCode)> StartSessionAsync(
        Guid currentUserId, StartWalkSessionRequest request);

    Task<(WalkRoutePointResponse? Point, string? ErrorCode)> AddLocationAsync(
        Guid currentUserId, Guid sessionId, AddLocationRequest request);

    Task<(WalkSessionResponse? Session, string? ErrorCode)> PauseSessionAsync(
        Guid currentUserId, Guid sessionId);

    Task<(WalkSessionResponse? Session, string? ErrorCode)> ResumeSessionAsync(
        Guid currentUserId, Guid sessionId);

    Task<(WalkSessionResponse? Session, string? ErrorCode)> CompleteSessionAsync(
        Guid currentUserId, Guid sessionId);

    Task<(IReadOnlyCollection<WalkRoutePointResponse>? Points, string? ErrorCode)> GetRouteAsync(
        Guid currentUserId, Guid sessionId);
}
