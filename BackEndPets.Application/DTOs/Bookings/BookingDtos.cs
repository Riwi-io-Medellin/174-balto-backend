namespace BackEndPets.Application.DTOs.Bookings;

public sealed record CreateBookingRequest(
    Guid WalkerId,
    Guid PetId,
    DateTime SlotStart,
    int DurationMinutes,
    string? SpecialInstructions,
    bool IsExclusive = false);

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
    bool IsExclusive,
    string? SpecialInstructions,
    Guid? WalkSessionId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    double? TotalDistanceMeters,
    int? TotalDurationSeconds,
    double? OwnerLatitude,
    double? OwnerLongitude,
    string? OwnerAddress,
    string? WalkerName = null,
    string? WalkerPhotoUrl = null);
