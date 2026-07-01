using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeProviderAvailabilityService(
    IHomeServiceProviderRepository providerRepository,
    IHomeServiceAvailabilityEngine engine) : IHomeProviderAvailabilityService
{
    public async Task<(IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>? Result, string? ErrorCode)> GetMyAvailabilityAsync(
        Guid userId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        return (await engine.GetWeeklyAsync(provider.Id), null);
    }

    public async Task<(IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>? Result, string? ErrorCode)> ReplaceMyAvailabilityAsync(
        Guid userId, IEnumerable<HomeProviderAvailabilitySlotRequest> slots)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        if (provider.VerificationStatus != "approved") return (null, "PROVIDER_NOT_APPROVED");

        return await engine.ReplaceWeeklyAsync(provider.Id, slots);
    }

    public async Task<(IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>? Result, string? ErrorCode)> GetMyExceptionsAsync(
        Guid userId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        return (await engine.GetExceptionsAsync(provider.Id), null);
    }

    public async Task<(IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>? Result, string? ErrorCode)> ReplaceMyExceptionsAsync(
        Guid userId, IEnumerable<HomeProviderAvailabilityExceptionRequest> exceptions)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        if (provider.VerificationStatus != "approved") return (null, "PROVIDER_NOT_APPROVED");

        return await engine.ReplaceExceptionsAsync(provider.Id, exceptions);
    }

    public async Task<(IReadOnlyCollection<HomeServiceAvailableSlotResponse>? Result, string? ErrorCode)> GetAvailableSlotsAsync(
        Guid providerId, DateOnly date, int durationMinutes)
    {
        if (durationMinutes is not (30 or 60 or 90))
            return (null, "INVALID_DURATION");

        var provider = await providerRepository.GetByIdAsync(providerId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");
        if (provider.VerificationStatus != "approved") return (null, "PROVIDER_NOT_APPROVED");
        if (!provider.IsAcceptingBookings) return (null, "PROVIDER_NOT_ACCEPTING_BOOKINGS");

        return (await engine.ComputeSlotsAsync(providerId, date, durationMinutes, provider.MaxConcurrentBookings), null);
    }
}
