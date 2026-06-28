namespace BackEndPets.Application.DTOs.Profiles;

public sealed record MeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsWalker,
    string? WalkerStatus,
    IReadOnlyCollection<BusinessSummary> Businesses);

public sealed record BusinessSummary(
    Guid Id,
    string Name,
    string? Type,
    string VerificationStatus);

public sealed record WalkerResponse(
    Guid Id,
    Guid UserId,
    string VerificationStatus,
    bool Available,
    string? WorkLocation,
    string? Experience,
    string? Description,
    DateTime CreatedAt);

public sealed record CreateBusinessRequest(
    string Name,
    string Nit,
    string Email,
    long Phone,
    string? Type = null,
    string? Location = null,
    string? Address = null);

public sealed record BusinessResponse(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string Nit,
    string Email,
    long Phone,
    string? Type,
    string? Location,
    string? Address,
    string VerificationStatus,
    DateTime CreatedAt);

public sealed record WalkerProfileResponse(
    Guid Id,
    Guid UserId,
    string VerificationStatus,
    bool Available,
    string? WorkLocation,
    string? Experience,
    string? Description,
    string? Bio,
    decimal? HourlyRate,
    decimal? ServiceRadiusKm,
    int? YearsOfExperience,
    bool IsAcceptingBookings,
    string? DocumentName,
    string? DocumentNumber,
    double? WorkLatitude,
    double? WorkLongitude,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record UpdateWalkerProfileRequest(
    string? Bio,
    decimal? HourlyRate,
    decimal? ServiceRadiusKm,
    int? YearsOfExperience,
    bool? IsAcceptingBookings,
    double? WorkLatitude,
    double? WorkLongitude);

public sealed record WalkerFilterRequest(
    bool? Available,
    string? WorkLocation);

public sealed record BusinessFilterRequest(
    string? Type, 
    string? Location);

public sealed record WalkerRecommendationRequest(
    string? WorkLocation = null);

public sealed record WalkerRecommendationResponse(
    Guid Id,
    Guid UserId,
    bool Available,
    string? WorkLocation,
    string? Experience,
    string? Description,
    string VerificationStatus,
    IReadOnlyCollection<string> Reasons);
