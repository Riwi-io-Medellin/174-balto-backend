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

// ── Booking-based session lifecycle ─────────────────────────────────────────

public sealed record StartSessionFromBookingRequest(Guid BookingId);

public sealed record FinishSessionRequest(double TotalDistanceMeters, int TotalDurationSeconds);

public sealed record BookingSessionResponse(
    Guid Id,
    Guid WalkerId,
    Guid BookingId,
    string Status,
    DateTime StartedAt,
    DateTime? FinishedAt,
    double? TotalDistanceMeters,
    int? TotalDurationSeconds);

// ── Walk session media ────────────────────────────────────────────────────────

public sealed record AddWalkMediaRequest(string Url, string Type);

public sealed record WalkSessionMediaResponse(
    Guid Id,
    Guid WalkSessionId,
    string Url,
    string Type,
    DateTime UploadedAt);

// ── Walk chat ──────────────────────────────────────────────────────────────

public sealed record SendChatMessageRequest(string Text);

public sealed record ChatMessageResponse(
    Guid Id,
    Guid WalkSessionId,
    Guid SenderUserId,
    string Text,
    DateTime CreatedAt);
