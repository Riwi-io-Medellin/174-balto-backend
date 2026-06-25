namespace BackEndPets.Application.DTOs.WalkSessions;

public sealed record StartWalkSessionRequest(IReadOnlyList<Guid> PetWalkingHistoryIds);

public sealed record AddLocationRequest(double Latitude, double Longitude);

public sealed record WalkSessionResponse(
    Guid Id,
    Guid? WalkerId,
    IReadOnlyList<Guid> PetWalkingHistoryIds,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt);

public sealed record WalkRoutePointResponse(
    Guid Id,
    Guid WalkSessionId,
    double Latitude,
    double Longitude,
    DateTime CreatedAt);

public sealed record LocationUpdatedPayload(
    Guid SessionId,
    double Latitude,
    double Longitude,
    DateTime Timestamp);
