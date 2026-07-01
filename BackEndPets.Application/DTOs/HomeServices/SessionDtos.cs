namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record AddHomeServiceSessionEventRequest(
    string EventType,
    string? Description,
    double? Latitude,
    double? Longitude);

public sealed record HomeServiceSessionEventResponse(
    Guid Id,
    Guid SessionId,
    string EventType,
    string? Description,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt);

public sealed record FinishHomeServiceSessionRequest(
    double? TotalDistanceMeters,
    int TotalDurationSeconds);

public sealed record HomeServiceSessionResponse(
    Guid Id,
    Guid ProviderId,
    Guid BookingId,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    double? TotalDistanceMeters,
    int? TotalDurationSeconds);
