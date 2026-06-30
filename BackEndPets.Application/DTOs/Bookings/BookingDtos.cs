namespace BackEndPets.Application.DTOs.Bookings;

public sealed record CreateBookingRequest(
    Guid WalkerId,
    Guid PetId,
    DateTime SlotStart,
    int DurationMinutes,
    string? SpecialInstructions);

public sealed record BookingResponse(
    Guid Id,
    Guid ClientUserId,
    Guid WalkerId,
    Guid PetId,
    string Status,
    DateTime SlotStart,
    int DurationMinutes,
    decimal? SnapshotHourlyRate,
    decimal? TotalPrice,
    string? SpecialInstructions,
    Guid? WalkSessionId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    double? TotalDistanceMeters,
    int? TotalDurationSeconds);
