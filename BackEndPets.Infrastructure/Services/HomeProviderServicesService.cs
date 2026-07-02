using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeProviderServicesService(
    IHomeServiceProviderRepository providerRepository,
    IHomeProviderServiceRepository serviceRepository,
    IHomeServiceTypeRepository typeRepository) : IHomeProviderServicesService
{
    private static readonly string[] ValidPriceUnits = ["flat", "hourly", "per_visit"];

    public async Task<(HomeProviderServiceResponse? Service, string? ErrorCode)> AddAsync(
        Guid userId, AddHomeProviderServiceRequest request)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var serviceType = await typeRepository.GetByIdAsync(request.ServiceTypeId);
        if (serviceType is null) return (null, "SERVICE_TYPE_NOT_FOUND");

        if (await serviceRepository.ExistsAsync(provider.Id, request.ServiceTypeId))
            return (null, "SERVICE_TYPE_ALREADY_OFFERED");

        if (request.Price.HasValue && request.Price.Value < 0) return (null, "INVALID_PRICE");
        if (!ValidPriceUnits.Contains(request.PriceUnit)) return (null, "INVALID_PRICE_UNIT");

        var service = new HomeProviderService
        {
            ProviderId = provider.Id,
            ServiceTypeId = request.ServiceTypeId,
            Price = request.Price,
            PriceUnit = request.PriceUnit,
            Description = request.Description?.Trim()
        };

        var created = await serviceRepository.CreateAsync(service);
        return (MapResponse(created, serviceType), null);
    }

    public async Task<IReadOnlyCollection<HomeProviderServiceResponse>> GetByProviderIdAsync(Guid providerId)
    {
        var services = await serviceRepository.GetByProviderIdAsync(providerId);
        var result = new List<HomeProviderServiceResponse>(services.Count);
        foreach (var service in services)
        {
            var serviceType = await typeRepository.GetByIdAsync(service.ServiceTypeId);
            if (serviceType is not null)
                result.Add(MapResponse(service, serviceType));
        }
        return result;
    }

    public async Task<(HomeProviderServiceResponse? Service, string? ErrorCode)> UpdateAsync(
        Guid userId, Guid serviceId, UpdateHomeProviderServiceRequest request)
    {
        var service = await serviceRepository.GetByIdAsync(serviceId);
        if (service is null) return (null, "SERVICE_NOT_FOUND");

        var provider = await providerRepository.GetByIdAsync(service.ProviderId);
        if (provider is null || provider.UserId != userId) return (null, "UNAUTHORIZED");

        if (request.Price.HasValue && request.Price.Value < 0) return (null, "INVALID_PRICE");
        if (!ValidPriceUnits.Contains(request.PriceUnit)) return (null, "INVALID_PRICE_UNIT");

        service.Price = request.Price;
        service.PriceUnit = request.PriceUnit;
        service.Description = request.Description?.Trim();
        service.IsActive = request.IsActive;

        var updated = await serviceRepository.UpdateAsync(service);
        var serviceType = await typeRepository.GetByIdAsync(updated!.ServiceTypeId);
        return (MapResponse(updated, serviceType!), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid serviceId)
    {
        var service = await serviceRepository.GetByIdAsync(serviceId);
        if (service is null) return (false, "SERVICE_NOT_FOUND");

        var provider = await providerRepository.GetByIdAsync(service.ProviderId);
        if (provider is null || provider.UserId != userId) return (false, "UNAUTHORIZED");

        await serviceRepository.DeleteAsync(serviceId);
        return (true, null);
    }

    private static HomeProviderServiceResponse MapResponse(HomeProviderService s, HomeServiceType t) =>
        new(s.Id, s.ProviderId, s.ServiceTypeId, t.Code, t.Name, s.Price, s.PriceUnit, s.Description, s.IsActive, s.CreatedAt);
}
