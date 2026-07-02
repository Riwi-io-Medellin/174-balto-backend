using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class AvailabilityEngine(
    IWalkerAvailabilityRepository availabilityRepository,
    IWalkerAvailabilityExceptionRepository exceptionRepository,
    IWalkBookingRepository bookingRepository) : IAvailabilityEngine
{
    // Every booking reserves pickup + walk + travel-to-next-service time, not just the walk itself.
    private const int PickupBufferMinutes = 15;
    private const int TravelBufferMinutes = 15;

    /// <summary>
    /// Total minutes a booking occupies on the walker's calendar, starting at its scheduled start.
    /// e.g. a 30-minute walk occupies 60 minutes (15 pickup + 30 walk + 15 travel).
    /// </summary>
    private static int OccupiedMinutes(int walkDurationMinutes) =>
        PickupBufferMinutes + walkDurationMinutes + TravelBufferMinutes;

    public async Task<IReadOnlyCollection<AvailabilitySlotResponse>> GetWeeklyAsync(Guid walkerId) =>
        (await availabilityRepository.GetByWalkerIdAsync(walkerId))
            .Select(MapSlot)
            .ToList();

    public async Task<(IReadOnlyCollection<AvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceWeeklyAsync(
        Guid walkerId, IEnumerable<AvailabilitySlotRequest> slots)
    {
        var list = slots.ToList();

        foreach (var slot in list)
        {
            if (slot.DayOfWeek is < 0 or > 6)
                return (null, "INVALID_DAY_OF_WEEK");

            if (slot.StartTime >= slot.EndTime)
                return (null, "INVALID_TIME_RANGE");
        }

        var entities = list.Select(s => new WalkerAvailability
        {
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime   = s.EndTime
        });

        var saved = await availabilityRepository.ReplaceAsync(walkerId, entities);
        return (saved.Select(MapSlot).ToList(), null);
    }

    public async Task<IReadOnlyCollection<AvailabilityExceptionResponse>> GetExceptionsAsync(Guid walkerId) =>
        (await exceptionRepository.GetByWalkerIdAsync(walkerId))
            .Select(MapException)
            .ToList();

    public async Task<(IReadOnlyCollection<AvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceExceptionsAsync(
        Guid walkerId, IEnumerable<AvailabilityExceptionRequest> exceptions)
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

        var entities = list.Select(e => new WalkerAvailabilityException
        {
            Date          = e.Date,
            IsUnavailable = e.IsUnavailable,
            StartTime     = e.IsUnavailable ? null : e.StartTime,
            EndTime       = e.IsUnavailable ? null : e.EndTime
        });

        var saved = await exceptionRepository.ReplaceAsync(walkerId, entities);
        return (saved.Select(MapException).ToList(), null);
    }

    public async Task<IReadOnlyCollection<AvailableSlotResponse>> ComputeSlotsAsync(
        Guid walkerId, DateOnly date, int durationMinutes, int? maxDogs = null)
    {
        var dayOfWeek = (int)date.DayOfWeek;

        // Load weekly schedule for this weekday
        var allSlots = await availabilityRepository.GetByWalkerIdAsync(walkerId);
        var daySlots = allSlots.Where(s => s.DayOfWeek == dayOfWeek).ToList();

        // Load exception for this specific date
        var allExceptions = await exceptionRepository.GetByWalkerIdAsync(walkerId);
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

        var occupiedMinutes = OccupiedMinutes(durationMinutes);
        var result = new List<AvailableSlotResponse>();

        foreach (var (ivStart, ivEnd) in intervals)
        {
            int startMin = ivStart.Hour * 60 + ivStart.Minute;
            int endMin   = ivEnd.Hour  * 60 + ivEnd.Minute;

            // The full occupied block (pickup + walk + travel) must fit inside the walker's
            // available window, otherwise travel time would spill past the end of their day.
            for (int t = startMin; t + occupiedMinutes <= endMin; t += 30)
            {
                var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(t));
                var slotEndTime   = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(t + durationMinutes));
                // Store Colombia local time directly — no UTC conversion needed.
                result.Add(new AvailableSlotResponse(
                    new DateTime(date.Year, date.Month, date.Day, slotStartTime.Hour, slotStartTime.Minute, 0),
                    new DateTime(date.Year, date.Month, date.Day, slotEndTime.Hour,   slotEndTime.Minute,   0)));
            }
        }

        // Exclude slots whose occupied interval (pickup + walk + travel) overlaps an active
        // booking's occupied interval — this is what actually keeps the walker's calendar clear.
        var allBookings = await bookingRepository.GetByWalkerIdAsync(walkerId, status: null);
        var activeBookings = allBookings
            .Where(b => b.Status is "pending" or "accepted")
            .ToList();

        var capacity = maxDogs ?? 1;

        return result
            .Where(slot =>
            {
                var slotOccupiedEnd = slot.Start.AddMinutes(occupiedMinutes);
                var overlapping = activeBookings.Count(b =>
                {
                    var bookingOccupiedEnd = b.SlotStart.AddMinutes(OccupiedMinutes(b.DurationMinutes));
                    return b.SlotStart < slotOccupiedEnd && bookingOccupiedEnd > slot.Start;
                });
                return overlapping < capacity;
            })
            .ToList();
    }

    private static AvailabilitySlotResponse MapSlot(WalkerAvailability a) =>
        new(a.Id, a.WalkerId, a.DayOfWeek, a.StartTime, a.EndTime);

    private static AvailabilityExceptionResponse MapException(WalkerAvailabilityException e) =>
        new(e.Id, e.WalkerId, e.Date, e.IsUnavailable, e.StartTime, e.EndTime);
}
