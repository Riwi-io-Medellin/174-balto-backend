using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkerMarketplaceService(
    IWalkerRepository walkerRepository,
    IFeedbackRepository feedbackRepository,
    IWalkBookingRepository bookingRepository,
    IAvailabilityEngine availabilityEngine) : IWalkerMarketplaceService
{
    public async Task<(PagedResult<WalkerSummaryResponse>? Result, string? ErrorCode)> SearchAsync(
        WalkerSearchQuery query)
    {
        if (query.DurationMinutes is not (30 or 60 or 90))
            return (null, "INVALID_DURATION");

        var page     = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        // Load all approved + accepting walkers with joined user data
        var walkers = await walkerRepository.GetApprovedAcceptingWithUserAsync();

        // Filter by radius — walkers without coordinates are excluded
        var inRadius = walkers
            .Where(p => p.Walker.WorkLatitude.HasValue && p.Walker.WorkLongitude.HasValue)
            .Select(p => (Projection: p,
                          Distance: Haversine(query.Latitude, query.Longitude,
                                              p.Walker.WorkLatitude!.Value,
                                              p.Walker.WorkLongitude!.Value)))
            .Where(x => x.Distance <= query.RadiusKm)
            .ToList();

        // Filter by availability — keep only walkers with at least one slot
        var withAvailability = new List<(WalkerUserProjection Projection, double Distance)>();
        foreach (var (proj, dist) in inRadius)
        {
            var slots = await availabilityEngine.ComputeSlotsAsync(proj.Walker.Id, query.Date, query.DurationMinutes);
            if (slots.Count > 0)
                withAvailability.Add((proj, dist));
        }

        var totalCount = withAvailability.Count;

        var paged = withAvailability
            .OrderBy(x => x.Distance)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<WalkerSummaryResponse>();
        foreach (var (proj, dist) in paged)
        {
            var feedbacks = await feedbackRepository.GetByTargetAsync(proj.Walker.Id, "walker");
            var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;

            items.Add(new WalkerSummaryResponse(
                proj.Walker.Id,
                $"{proj.FirstName} {proj.LastName}",
                proj.PhotoUrl,
                proj.Walker.Bio,
                proj.Walker.HourlyRate,
                Math.Round(avg, 1),
                feedbacks.Count,
                proj.Walker.YearsOfExperience,
                proj.Walker.ServiceRadiusKm,
                Math.Round(dist, 2),
                HasAvailability: true));
        }

        return (new PagedResult<WalkerSummaryResponse>(items, page, pageSize, totalCount), null);
    }

    public async Task<(WalkerDetailResponse? Result, string? ErrorCode)> GetDetailAsync(
        Guid walkerId, DateOnly? date, int? durationMinutes)
    {
        var proj = await walkerRepository.GetByIdWithUserAsync(walkerId);
        if (proj is null) return (null, "WALKER_NOT_FOUND");

        var walker = proj.Walker;

        var feedbacks = await feedbackRepository.GetByTargetAsync(walker.Id, "walker");
        var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;

        var completedBookings = await bookingRepository.GetByWalkerIdAsync(walker.Id, "completed");
        var weeklySlots       = await availabilityEngine.GetWeeklyAsync(walker.Id);

        IReadOnlyCollection<AvailableSlotResponse> availableSlots = [];
        if (date.HasValue && durationMinutes is 30 or 60 or 90)
            availableSlots = await availabilityEngine.ComputeSlotsAsync(walker.Id, date.Value, durationMinutes!.Value);

        return (new WalkerDetailResponse(
            walker.Id,
            walker.UserId,
            $"{proj.FirstName} {proj.LastName}",
            proj.PhotoUrl,
            walker.Bio,
            walker.HourlyRate,
            walker.ServiceRadiusKm,
            walker.YearsOfExperience,
            walker.WorkLocation,
            Math.Round(avg, 1),
            feedbacks.Count,
            completedBookings.Count,
            weeklySlots,
            availableSlots), null);
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
    }
}
