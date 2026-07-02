namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record HomeServiceProviderResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string? ProfilePhoto,
    string VerificationStatus,
    bool IsAcceptingBookings,
    string? BaseLocation,
    string? Experience,
    string? Description,
    DateTime CreatedAt,
    double AverageRating = 0,
    int TotalReviews = 0);

public sealed record HomeServiceProviderProfileResponse(
    Guid Id,
    Guid UserId,
    string VerificationStatus,
    bool IsAcceptingBookings,
    string? BaseLocation,
    string? Experience,
    string? Description,
    string? Bio,
    int? YearsOfExperience,
    int MaxConcurrentBookings,
    string? DocumentName,
    string? DocumentNumber,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record UpdateHomeServiceProviderRequest(
    string? Bio,
    int? YearsOfExperience,
    bool? IsAcceptingBookings,
    int? MaxConcurrentBookings,
    string? BaseLocation,
    double? Latitude,
    double? Longitude);

public sealed record HomeServiceProviderFilterRequest(
    bool? IsAcceptingBookings,
    string? BaseLocation);

public sealed record HomeServiceApplyRequest(
    string BaseLocation,
    string Experience,
    string? Description);

public sealed record HomeServiceApplyResponse(
    Guid ProviderId,
    Guid UserId,
    string VerificationStatus,
    string DocumentUrl,
    string? DocumentName,
    string? DocumentNumber,
    string Message);
