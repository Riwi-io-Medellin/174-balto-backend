using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

/// <summary>
/// Core availability engine for Home Service providers. Parallel implementation
/// of IAvailabilityEngine (not shared with it — that engine is hard-coupled to
/// Walker repositories/MaxDogs capacity semantics). Operates on providerId directly.
/// </summary>
public interface IHomeServiceAvailabilityEngine
{
    Task<IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>> GetWeeklyAsync(Guid providerId);

    /// <summary>
    /// Validates and replaces the full weekly schedule for a provider.
    /// Validation: dayOfWeek 0–6, startTime &lt; endTime.
    /// </summary>
    Task<(IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceWeeklyAsync(
        Guid providerId, IEnumerable<HomeProviderAvailabilitySlotRequest> slots);

    Task<IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>> GetExceptionsAsync(Guid providerId);

    /// <summary>
    /// Validates and replaces all date exceptions for a provider.
    /// Validation: no duplicate dates, times required when IsUnavailable = false.
    /// </summary>
    Task<(IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceExceptionsAsync(
        Guid providerId, IEnumerable<HomeProviderAvailabilityExceptionRequest> exceptions);

    /// <summary>
    /// Generates bookable time slots for a provider on a given date.
    /// Applies exceptions on top of weekly schedule. Slots step every 30 min.
    /// Allowed durations: 30, 60, 90.
    /// When maxConcurrentBookings is provided, slots already at capacity
    /// (overlapping active bookings >= maxConcurrentBookings) are excluded.
    /// </summary>
    Task<IReadOnlyCollection<HomeServiceAvailableSlotResponse>> ComputeSlotsAsync(
        Guid providerId, DateOnly date, int durationMinutes, int? maxConcurrentBookings = null);
}
