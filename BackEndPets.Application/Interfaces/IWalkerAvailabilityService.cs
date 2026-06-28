using BackEndPets.Application.DTOs.Walkers;

namespace BackEndPets.Application.Interfaces;

/// <summary>
/// User-facing service. Resolves userId → approved walker, then delegates to IAvailabilityEngine.
/// </summary>
public interface IWalkerAvailabilityService
{
    Task<(IReadOnlyCollection<AvailabilitySlotResponse>? Result, string? ErrorCode)> GetMyAvailabilityAsync(
        Guid userId);

    Task<(IReadOnlyCollection<AvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceMyAvailabilityAsync(
        Guid userId, IEnumerable<AvailabilitySlotRequest> slots);

    Task<(IReadOnlyCollection<AvailabilityExceptionResponse>? Result, string? ErrorCode)> GetMyExceptionsAsync(
        Guid userId);

    Task<(IReadOnlyCollection<AvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceMyExceptionsAsync(
        Guid userId, IEnumerable<AvailabilityExceptionRequest> exceptions);

    /// <summary>
    /// Validates walker (exists, approved, accepting bookings) then computes available slots.
    /// walkerId is the Walker entity id, not the user id.
    /// </summary>
    Task<(IReadOnlyCollection<AvailableSlotResponse>? Result, string? ErrorCode)> GetAvailableSlotsAsync(
        Guid walkerId, DateOnly date, int durationMinutes);
}
