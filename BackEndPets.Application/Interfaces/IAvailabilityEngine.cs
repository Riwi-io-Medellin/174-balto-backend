using BackEndPets.Application.DTOs.Walkers;

namespace BackEndPets.Application.Interfaces;

/// <summary>
/// Core availability engine. Operates on walkerId directly so it can be reused
/// by booking calculation, admin tooling, or any other caller without coupling
/// to user identity or authentication.
/// </summary>
public interface IAvailabilityEngine
{
    Task<IReadOnlyCollection<AvailabilitySlotResponse>> GetWeeklyAsync(Guid walkerId);

    /// <summary>
    /// Validates and replaces the full weekly schedule for a walker.
    /// Validation: dayOfWeek 0–6, startTime &lt; endTime.
    /// </summary>
    Task<(IReadOnlyCollection<AvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceWeeklyAsync(
        Guid walkerId, IEnumerable<AvailabilitySlotRequest> slots);

    Task<IReadOnlyCollection<AvailabilityExceptionResponse>> GetExceptionsAsync(Guid walkerId);

    /// <summary>
    /// Validates and replaces all date exceptions for a walker.
    /// Validation: no duplicate dates, times required when IsUnavailable = false.
    /// </summary>
    Task<(IReadOnlyCollection<AvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceExceptionsAsync(
        Guid walkerId, IEnumerable<AvailabilityExceptionRequest> exceptions);

    /// <summary>
    /// Generates bookable time slots for a walker on a given date.
    /// Applies exceptions on top of weekly schedule. Slots step every 30 min.
    /// Allowed durations: 30, 60, 90.
    /// When maxDogs is provided, slots already at capacity (overlapping active bookings >= maxDogs) are excluded.
    /// </summary>
    Task<IReadOnlyCollection<AvailableSlotResponse>> ComputeSlotsAsync(
        Guid walkerId, DateOnly date, int durationMinutes, int? maxDogs = null);
}
