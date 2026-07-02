using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeProviderServiceAreaService(
    IHomeServiceProviderRepository providerRepository,
    IHomeProviderServiceAreaRepository areaRepository) : IHomeProviderServiceAreaService
{
    public async Task<(IReadOnlyCollection<HomeProviderServiceAreaResponse>? Result, string? ErrorCode)> GetMyServiceAreasAsync(
        Guid userId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        return ((await areaRepository.GetByProviderIdAsync(provider.Id)).Select(MapArea).ToList(), null);
    }

    public async Task<(IReadOnlyCollection<HomeProviderServiceAreaResponse>? Result, string? ErrorCode)> ReplaceMyServiceAreasAsync(
        Guid userId, IEnumerable<HomeProviderServiceAreaRequest> areas)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        if (provider.VerificationStatus != "approved") return (null, "PROVIDER_NOT_APPROVED");

        var list = areas.ToList();
        foreach (var area in list)
        {
            if (area.RadiusKm <= 0) return (null, "INVALID_RADIUS");
        }

        var entities = list.Select(a => new HomeProviderServiceArea
        {
            Label = a.Label?.Trim(),
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            RadiusKm = a.RadiusKm
        });

        var saved = await areaRepository.ReplaceAsync(provider.Id, entities);
        return (saved.Select(MapArea).ToList(), null);
    }

    private static HomeProviderServiceAreaResponse MapArea(HomeProviderServiceArea a) =>
        new(a.Id, a.ProviderId, a.Label, a.Latitude, a.Longitude, a.RadiusKm, a.CreatedAt);
}
