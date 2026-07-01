namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record HomeServiceSearchQuery(
    double Latitude,
    double Longitude,
    double RadiusKm,
    Guid? ServiceTypeId,
    DateOnly Date,
    int DurationMinutes,
    int Page = 1,
    int PageSize = 20);

public sealed record HomeServiceProviderSummaryResponse(
    Guid Id,
    string FullName,
    string? ProfilePhoto,
    string? Bio,
    double AverageRating,
    int TotalReviews,
    int? YearsOfExperience,
    double DistanceKm,
    bool HasAvailability,
    IReadOnlyCollection<HomeProviderServiceResponse> Services);

public sealed record HomeServiceProviderDetailResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string? ProfilePhoto,
    string? Bio,
    int? YearsOfExperience,
    string? BaseLocation,
    double AverageRating,
    int TotalReviews,
    int CompletedBookings,
    IReadOnlyCollection<HomeProviderServiceResponse> Services,
    IReadOnlyCollection<HomeProviderSpecialtyResponse> Specialties,
    IReadOnlyCollection<HomeProviderServiceAreaResponse> ServiceAreas,
    IReadOnlyCollection<HomeProviderAvailabilitySlotResponse> WeeklyAvailability,
    IReadOnlyCollection<HomeServiceAvailableSlotResponse> AvailableSlots);
