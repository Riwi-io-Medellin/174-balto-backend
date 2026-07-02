using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceAvailabilityEngine(
    IHomeProviderAvailabilityRepository availabilityRepository,
    IHomeProviderAvailabilityExceptionRepository exceptionRepository,
    IHomeServiceBookingRepository bookingRepository) : IHomeServiceAvailabilityEngine
{
    public async Task<IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>> GetWeeklyAsync(Guid providerId) =>
        (await availabilityRepository.GetByProviderIdAsync(providerId))
            .Select(MapSlot)
            .ToList();

    public async Task<(IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceWeeklyAsync(
        Guid providerId, IEnumerable<HomeProviderAvailabilitySlotRequest> slots)
    {
        var list = slots.ToList();

        foreach (var slot in list)
        {
            if (slot.DayOfWeek is < 0 or > 6)
                return (null, "INVALID_DAY_OF_WEEK");

            if (slot.StartTime >= slot.EndTime)
                return (null, "INVALID_TIME_RANGE");
        }

        var entities = list.Select(s => new HomeProviderAvailability
        {
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime   = s.EndTime
        });

        var saved = await availabilityRepository.ReplaceAsync(providerId, entities);
        return (saved.Select(MapSlot).ToList(), null);
    }

    public async Task<IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>> GetExceptionsAsync(Guid providerId) =>
        (await exceptionRepository.GetByProviderIdAsync(providerId))
            .Select(MapException)
            .ToList();

    public async Task<(IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceExceptionsAsync(
        Guid providerId, IEnumerable<HomeProviderAvailabilityExceptionRequest> exceptions)
    {
        var list = exceptions.ToList();

        // Duplicate date check
        var duplicateDate = list
            .GroupBy(e => e.Date)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateDate is not null)
            return (null, "DUPLICATE_DATE");

        foreach (var ex in list)
        {
            if (!ex.IsUnavailable)
            {
                // Available day with alternate hours — times are mandatory
                if (ex.StartTime is null || ex.EndTime is null)
                    return (null, "TIMES_REQUIRED_WHEN_AVAILABLE");

                if (ex.StartTime.Value >= ex.EndTime.Value)
                    return (null, "INVALID_TIME_RANGE");
            }
        }

        var entities = list.Select(e => new HomeProviderAvailabilityException
        {
            Date          = e.Date,
            IsUnavailable = e.IsUnavailable,
            StartTime     = e.IsUnavailable ? null : e.StartTime,
            EndTime       = e.IsUnavailable ? null : e.EndTime,
            Reason        = e.Reason?.Trim()
        });

        var saved = await exceptionRepository.ReplaceAsync(providerId, entities);
        return (saved.Select(MapException).ToList(), null);
    }

    public async Task<IReadOnlyCollection<HomeServiceAvailableSlotResponse>> ComputeSlotsAsync(
        Guid providerId, DateOnly date, int durationMinutes, int? maxConcurrentBookings = null)
    {
        var dayOfWeek = (int)date.DayOfWeek;

        // Load weekly schedule for this weekday
        var allSlots = await availabilityRepository.GetByProviderIdAsync(providerId);
        var daySlots = allSlots.Where(s => s.DayOfWeek == dayOfWeek).ToList();

        // Load exception for this specific date
        var allExceptions = await exceptionRepository.GetByProviderIdAsync(providerId);
        var exception = allExceptions.FirstOrDefault(e => e.Date == date);

        // Determine effective intervals for the day
        List<(TimeOnly Start, TimeOnly End)> intervals;

        if (exception is not null)
        {
            if (exception.IsUnavailable) return [];
            intervals = [(exception.StartTime!.Value, exception.EndTime!.Value)];
        }
        else
        {
            intervals = daySlots.Select(s => (s.StartTime, s.EndTime)).ToList();
        }

        var result = new List<HomeServiceAvailableSlotResponse>();

        foreach (var (ivStart, ivEnd) in intervals)
        {
            int startMin = ivStart.Hour * 60 + ivStart.Minute;
            int endMin   = ivEnd.Hour  * 60 + ivEnd.Minute;

            for (int t = startMin; t + durationMinutes <= endMin; t += 30)
            {
                var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(t));
                var slotEndTime   = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(t + durationMinutes));
                // Store Colombia local time directly — no UTC conversion needed.
                result.Add(new HomeServiceAvailableSlotResponse(
                    new DateTime(date.Year, date.Month, date.Day, slotStartTime.Hour, slotStartTime.Minute, 0),
                    new DateTime(date.Year, date.Month, date.Day, slotEndTime.Hour,   slotEndTime.Minute,   0)));
            }
        }

        if (maxConcurrentBookings is null)
            return result;

        // Filter out slots where active bookings (pending/accepted) already fill capacity
        var allBookings = await bookingRepository.GetByProviderIdAsync(providerId, status: null);
        var activeBookings = allBookings
            .Where(b => b.Status is "pending" or "accepted")
            .ToList();

        return result
            .Where(slot =>
            {
                var overlapping = activeBookings.Count(b =>
                {
                    var bookingEnd = b.SlotStart.AddMinutes(b.DurationMinutes);
                    return b.SlotStart < slot.End && bookingEnd > slot.Start;
                });
                return overlapping < maxConcurrentBookings.Value;
            })
            .ToList();
    }

    private static HomeProviderAvailabilitySlotResponse MapSlot(HomeProviderAvailability a) =>
        new(a.Id, a.ProviderId, a.DayOfWeek, a.StartTime, a.EndTime);

    private static HomeProviderAvailabilityExceptionResponse MapException(HomeProviderAvailabilityException e) =>
        new(e.Id, e.ProviderId, e.Date, e.IsUnavailable, e.StartTime, e.EndTime, e.Reason);
}
