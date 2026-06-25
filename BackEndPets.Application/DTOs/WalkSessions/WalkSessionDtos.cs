namespace BackEndPets.Application.DTOs.WalkSessions;

public sealed record StartWalkSessionRequest(Guid PetWalkingHistoryId);

public sealed record AddLocationRequest(double Latitude, double Longitude);

public sealed record WalkSessionResponse(
    Guid Id,
    Guid PetWalkingHistoryId,
    string Status,
    DateTime? StartedAt,
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
