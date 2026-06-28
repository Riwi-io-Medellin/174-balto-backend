using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkerAvailabilityService(
    IWalkerRepository walkerRepository,
    IAvailabilityEngine engine) : IWalkerAvailabilityService
{
    public async Task<(IReadOnlyCollection<AvailabilitySlotResponse>? Result, string? ErrorCode)> GetMyAvailabilityAsync(
        Guid userId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        return (await engine.GetWeeklyAsync(walker.Id), null);
    }

    public async Task<(IReadOnlyCollection<AvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceMyAvailabilityAsync(
        Guid userId, IEnumerable<AvailabilitySlotRequest> slots)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        if (walker.VerificationStatus != "approved") return (null, "WALKER_NOT_APPROVED");

        return await engine.ReplaceWeeklyAsync(walker.Id, slots);
    }

    public async Task<(IReadOnlyCollection<AvailabilityExceptionResponse>? Result, string? ErrorCode)> GetMyExceptionsAsync(
        Guid userId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        return (await engine.GetExceptionsAsync(walker.Id), null);
    }

    public async Task<(IReadOnlyCollection<AvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceMyExceptionsAsync(
        Guid userId, IEnumerable<AvailabilityExceptionRequest> exceptions)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        if (walker.VerificationStatus != "approved") return (null, "WALKER_NOT_APPROVED");

        return await engine.ReplaceExceptionsAsync(walker.Id, exceptions);
    }

    public async Task<(IReadOnlyCollection<AvailableSlotResponse>? Result, string? ErrorCode)> GetAvailableSlotsAsync(
        Guid walkerId, DateOnly date, int durationMinutes)
    {
        if (durationMinutes is not (30 or 60 or 90))
            return (null, "INVALID_DURATION");

        var walker = await walkerRepository.GetByIdAsync(walkerId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");
        if (walker.VerificationStatus != "approved") return (null, "WALKER_NOT_APPROVED");
        if (!walker.IsAcceptingBookings) return (null, "WALKER_NOT_ACCEPTING_BOOKINGS");

        return (await engine.ComputeSlotsAsync(walkerId, date, durationMinutes), null);
    }
}
