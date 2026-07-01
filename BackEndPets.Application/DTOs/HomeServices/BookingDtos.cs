namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record CreateHomeServiceBookingRequest(
    Guid ProviderId,
    Guid ServiceTypeId,
    Guid PetId,
    DateTime SlotStart,
    int DurationMinutes,
    string? ServiceAddress,
    double? ServiceLatitude,
    double? ServiceLongitude,
    string? SpecialInstructions);

public sealed record HomeServiceBookingResponse(
    Guid Id,
    Guid ClientUserId,
    Guid ProviderId,
    Guid ServiceTypeId,
    Guid PetId,
    string Status,
    DateTime SlotStart,
    int DurationMinutes,
    decimal? SnapshotPrice,
    decimal? TotalPrice,
    string? ServiceAddress,
    double? ServiceLatitude,
    double? ServiceLongitude,
    string? SpecialInstructions,
    Guid? HomeServiceSessionId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? ProviderName = null,
    string? ProviderPhotoUrl = null);
