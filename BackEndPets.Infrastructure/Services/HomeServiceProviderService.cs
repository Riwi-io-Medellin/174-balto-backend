using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceProviderService(
    UserManager<ApplicationUser> userManager,
    IHomeServiceProviderRepository providerRepository,
    IFeedbackRepository feedbackRepository) : IHomeServiceProviderService
{
    private const string TargetType = "home_service_provider";

    public async Task<(HomeServiceProviderResponse? Provider, string? ErrorCode)> BecomeProviderAsync(Guid userId)
    {
        if (await providerRepository.ExistsForUserAsync(userId))
            return (null, "PROVIDER_ALREADY_EXISTS");

        var created = await providerRepository.CreateAsync(new HomeServiceProvider { UserId = userId });

        var user = await userManager.FindByIdAsync(userId.ToString());
        return (new HomeServiceProviderResponse(
            created.Id,
            created.UserId,
            $"{user?.FirstName ?? ""} {user?.LastName ?? ""}".Trim(),
            user?.PhotoUrl,
            created.VerificationStatus,
            created.IsAcceptingBookings,
            created.BaseLocation,
            created.Experience,
            created.Description,
            created.CreatedAt), null);
    }

    public async Task<IReadOnlyCollection<HomeServiceProviderResponse>> GetProvidersAsync(
        HomeServiceProviderFilterRequest filters)
    {
        var projections = await providerRepository.GetAllWithUserAsync(
            filters.IsAcceptingBookings, filters.BaseLocation);
        var providerIds = projections.Select(p => p.Provider.Id).ToList();
        var feedbacks = await feedbackRepository.GetByTargetsAsync(providerIds, TargetType);
        var ratingMap = feedbacks
            .GroupBy(f => f.TargetId)
            .ToDictionary(g => g.Key, g => (Average: g.Average(f => f.Rating), Count: g.Count()));

        return projections.Select(p =>
        {
            var (avg, count) = ratingMap.GetValueOrDefault(p.Provider.Id, (0.0, 0));
            return new HomeServiceProviderResponse(
                p.Provider.Id, p.Provider.UserId,
                $"{p.FirstName} {p.LastName}", p.PhotoUrl,
                p.Provider.VerificationStatus, p.Provider.IsAcceptingBookings,
                p.Provider.BaseLocation, p.Provider.Experience,
                p.Provider.Description, p.Provider.CreatedAt,
                Math.Round(avg, 1), count);
        }).ToList();
    }

    public async Task<HomeServiceProviderResponse?> GetProviderByIdAsync(Guid id)
    {
        var projection = await providerRepository.GetByIdWithUserAsync(id);
        if (projection is null) return null;

        var feedbacks = await feedbackRepository.GetByTargetAsync(id, TargetType);
        var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;

        return new HomeServiceProviderResponse(
            projection.Provider.Id, projection.Provider.UserId,
            $"{projection.FirstName} {projection.LastName}", projection.PhotoUrl,
            projection.Provider.VerificationStatus, projection.Provider.IsAcceptingBookings,
            projection.Provider.BaseLocation, projection.Provider.Experience,
            projection.Provider.Description, projection.Provider.CreatedAt,
            Math.Round(avg, 1), feedbacks.Count);
    }

    public async Task<(HomeServiceProviderProfileResponse? Profile, string? ErrorCode)> GetMyProviderProfileAsync(
        Guid userId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        return provider is null
            ? (null, "PROVIDER_NOT_FOUND")
            : (MapProfile(provider), null);
    }

    public async Task<(HomeServiceProviderProfileResponse? Profile, string? ErrorCode)> UpdateMyProviderProfileAsync(
        Guid userId, UpdateHomeServiceProviderRequest request)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null)
            return (null, "PROVIDER_NOT_FOUND");

        if (provider.VerificationStatus != "approved")
            return (null, "PROVIDER_NOT_APPROVED");

        if (request.YearsOfExperience.HasValue && request.YearsOfExperience.Value < 0)
            return (null, "YEARS_OF_EXPERIENCE_INVALID");

        if (request.MaxConcurrentBookings.HasValue && request.MaxConcurrentBookings.Value < 1)
            return (null, "MAX_CONCURRENT_BOOKINGS_INVALID");

        if (request.Bio is not null)
            provider.Bio = request.Bio.Trim();

        if (request.YearsOfExperience.HasValue)
            provider.YearsOfExperience = request.YearsOfExperience.Value;

        if (request.IsAcceptingBookings.HasValue)
            provider.IsAcceptingBookings = request.IsAcceptingBookings.Value;

        if (request.MaxConcurrentBookings.HasValue)
            provider.MaxConcurrentBookings = request.MaxConcurrentBookings.Value;

        if (request.BaseLocation is not null)
            provider.BaseLocation = request.BaseLocation.Trim();

        if (request.Latitude.HasValue)
            provider.Latitude = request.Latitude.Value;

        if (request.Longitude.HasValue)
            provider.Longitude = request.Longitude.Value;

        await providerRepository.UpdateAsync(provider);
        return (MapProfile(provider), null);
    }

    private static HomeServiceProviderProfileResponse MapProfile(HomeServiceProvider p) => new(
        p.Id, p.UserId, p.VerificationStatus,
        p.IsAcceptingBookings, p.BaseLocation, p.Experience, p.Description,
        p.Bio, p.YearsOfExperience, p.MaxConcurrentBookings,
        p.DocumentName, p.DocumentNumber,
        p.Latitude, p.Longitude,
        p.CreatedAt, p.UpdatedAt);
}
