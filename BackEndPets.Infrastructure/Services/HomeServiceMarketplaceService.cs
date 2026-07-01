using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceMarketplaceService(
    IHomeServiceProviderRepository providerRepository,
    IFeedbackRepository feedbackRepository,
    IHomeServiceBookingRepository bookingRepository,
    IHomeServiceAvailabilityEngine availabilityEngine,
    IHomeProviderServiceRepository providerServiceRepository,
    IHomeServiceTypeRepository serviceTypeRepository,
    IHomeProviderServiceAreaRepository serviceAreaRepository,
    IHomeProviderSpecialtyRepository specialtyRepository) : IHomeServiceMarketplaceService
{
    private const string TargetType = "home_service_provider";

    public async Task<(PagedResult<HomeServiceProviderSummaryResponse>? Result, string? ErrorCode)> SearchAsync(
        HomeServiceSearchQuery query)
    {
        if (query.DurationMinutes is not (30 or 60 or 90))
            return (null, "INVALID_DURATION");

        var page     = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        // Load all approved + accepting providers with joined user data
        var providers = await providerRepository.GetApprovedAcceptingWithUserAsync();

        var candidates = new List<(HomeServiceProviderUserProjection Projection, double Distance)>();
        foreach (var proj in providers)
        {
            var areas = await serviceAreaRepository.GetByProviderIdAsync(proj.Provider.Id);
            if (areas.Count == 0) continue;

            // Closest service area within both the client's requested radius and the
            // provider's own advertised radius for that area.
            double? bestDistance = null;
            foreach (var area in areas)
            {
                var distance = GeoUtils.Haversine(query.Latitude, query.Longitude, area.Latitude, area.Longitude);
                if (distance > query.RadiusKm || distance > (double)area.RadiusKm) continue;
                if (bestDistance is null || distance < bestDistance.Value) bestDistance = distance;
            }

            if (bestDistance is null) continue;

            if (query.ServiceTypeId.HasValue)
            {
                var offersType = await providerServiceRepository.ExistsAsync(proj.Provider.Id, query.ServiceTypeId.Value);
                if (!offersType) continue;
            }
            else
            {
                var services = await providerServiceRepository.GetByProviderIdAsync(proj.Provider.Id);
                if (!services.Any(s => s.IsActive)) continue;
            }

            candidates.Add((proj, bestDistance.Value));
        }

        // Filter by availability — keep only providers with at least one slot
        var withAvailability = new List<(HomeServiceProviderUserProjection Projection, double Distance)>();
        foreach (var (proj, dist) in candidates)
        {
            var slots = await availabilityEngine.ComputeSlotsAsync(
                proj.Provider.Id, query.Date, query.DurationMinutes, proj.Provider.MaxConcurrentBookings);
            if (slots.Count > 0)
                withAvailability.Add((proj, dist));
        }

        var totalCount = withAvailability.Count;

        var paged = withAvailability
            .OrderBy(x => x.Distance)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<HomeServiceProviderSummaryResponse>();
        foreach (var (proj, dist) in paged)
        {
            var feedbacks = await feedbackRepository.GetByTargetAsync(proj.Provider.Id, TargetType);
            var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;
            var services = await MapServicesAsync(proj.Provider.Id);

            items.Add(new HomeServiceProviderSummaryResponse(
                proj.Provider.Id,
                $"{proj.FirstName} {proj.LastName}",
                proj.PhotoUrl,
                proj.Provider.Bio,
                Math.Round(avg, 1),
                feedbacks.Count,
                proj.Provider.YearsOfExperience,
                Math.Round(dist, 2),
                HasAvailability: true,
                services));
        }

        return (new PagedResult<HomeServiceProviderSummaryResponse>(items, page, pageSize, totalCount), null);
    }

    public async Task<(HomeServiceProviderDetailResponse? Result, string? ErrorCode)> GetDetailAsync(
        Guid providerId, DateOnly? date, int? durationMinutes)
    {
        var proj = await providerRepository.GetByIdWithUserAsync(providerId);
        if (proj is null) return (null, "PROVIDER_NOT_FOUND");

        var provider = proj.Provider;

        var feedbacks = await feedbackRepository.GetByTargetAsync(provider.Id, TargetType);
        var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;

        var completedBookings = await bookingRepository.GetByProviderIdAsync(provider.Id, "completed");
        var weeklySlots       = await availabilityEngine.GetWeeklyAsync(provider.Id);
        var services          = await MapServicesAsync(provider.Id);
        var specialties       = (await specialtyRepository.GetByProviderIdAsync(provider.Id))
            .Select(s => new HomeProviderSpecialtyResponse(s.Id, s.ProviderId, s.Specialty, s.CreatedAt))
            .ToList();
        var serviceAreas      = (await serviceAreaRepository.GetByProviderIdAsync(provider.Id))
            .Select(a => new HomeProviderServiceAreaResponse(a.Id, a.ProviderId, a.Label, a.Latitude, a.Longitude, a.RadiusKm, a.CreatedAt))
            .ToList();

        IReadOnlyCollection<HomeServiceAvailableSlotResponse> availableSlots = [];
        if (date.HasValue && durationMinutes is 30 or 60 or 90)
            availableSlots = await availabilityEngine.ComputeSlotsAsync(
                provider.Id, date.Value, durationMinutes!.Value, provider.MaxConcurrentBookings);

        return (new HomeServiceProviderDetailResponse(
            provider.Id,
            provider.UserId,
            $"{proj.FirstName} {proj.LastName}",
            proj.PhotoUrl,
            provider.Bio,
            provider.YearsOfExperience,
            provider.BaseLocation,
            Math.Round(avg, 1),
            feedbacks.Count,
            completedBookings.Count,
            services,
            specialties,
            serviceAreas,
            weeklySlots,
            availableSlots), null);
    }

    private async Task<IReadOnlyCollection<HomeProviderServiceResponse>> MapServicesAsync(Guid providerId)
    {
        var services = await providerServiceRepository.GetByProviderIdAsync(providerId);
        var result = new List<HomeProviderServiceResponse>();
        foreach (var service in services.Where(s => s.IsActive))
        {
            var type = await serviceTypeRepository.GetByIdAsync(service.ServiceTypeId);
            if (type is not null)
                result.Add(new HomeProviderServiceResponse(
                    service.Id, service.ProviderId, service.ServiceTypeId, type.Code, type.Name,
                    service.Price, service.PriceUnit, service.Description, service.IsActive, service.CreatedAt));
        }
        return result;
    }
}
