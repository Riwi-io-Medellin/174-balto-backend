using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

/// <summary>
/// User-facing service. Resolves userId → approved provider, then delegates to IHomeServiceAvailabilityEngine.
/// </summary>
public interface IHomeProviderAvailabilityService
{
    Task<(IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>? Result, string? ErrorCode)> GetMyAvailabilityAsync(
        Guid userId);

    Task<(IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceMyAvailabilityAsync(
        Guid userId, IEnumerable<HomeProviderAvailabilitySlotRequest> slots);

    Task<(IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>? Result, string? ErrorCode)> GetMyExceptionsAsync(
        Guid userId);

    Task<(IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceMyExceptionsAsync(
        Guid userId, IEnumerable<HomeProviderAvailabilityExceptionRequest> exceptions);

    /// <summary>
    /// Validates provider (exists, approved, accepting bookings) then computes available slots.
    /// providerId is the HomeServiceProvider entity id, not the user id.
    /// </summary>
    Task<(IReadOnlyCollection<HomeServiceAvailableSlotResponse>? Result, string? ErrorCode)> GetAvailableSlotsAsync(
        Guid providerId, DateOnly date, int durationMinutes);
}
