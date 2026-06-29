using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Feedback;

namespace BackEndPets.Application.DTOs.Walkers;

public sealed record WalkerSearchQuery(
    double Latitude,
    double Longitude,
    double RadiusKm,
    DateOnly Date,
    int DurationMinutes,
    int Page = 1,
    int PageSize = 20);

public sealed record WalkerSummaryResponse(
    Guid Id,
    string FullName,
    string? ProfilePhoto,
    string? Bio,
    decimal? HourlyRate,
    double AverageRating,
    int TotalReviews,
    int? YearsOfExperience,
    decimal? ServiceRadiusKm,
    double DistanceKm,
    bool HasAvailability);

public sealed record WalkerDetailResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string? ProfilePhoto,
    string? Bio,
    decimal? HourlyRate,
    decimal? ServiceRadiusKm,
    int? YearsOfExperience,
    string? WorkLocation,
    double AverageRating,
    int TotalReviews,
    int CompletedWalks,
    IReadOnlyCollection<AvailabilitySlotResponse> WeeklyAvailability,
    IReadOnlyCollection<AvailableSlotResponse> AvailableSlots);


