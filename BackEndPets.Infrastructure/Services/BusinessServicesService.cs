using BackEndPets.Application.DTOs.Businesses;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class BusinessServicesService(
    IBusinessRepository businessRepository,
    IBusinessServiceRepository serviceRepository) : IBusinessServicesService
{
    private static readonly string[] ValidItemKinds = ["service", "product"];

    public async Task<(BusinessServiceResponse? Service, string? ErrorCode)> CreateAsync(
        Guid userId, Guid businessId, CreateBusinessServiceRequest request)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business is null) return (null, "BUSINESS_NOT_FOUND");
        if (business.OwnerUserId != userId) return (null, "UNAUTHORIZED");

        if (request.Price < 0) return (null, "INVALID_PRICE");

        var itemKind = request.ItemKind?.Trim().ToLowerInvariant() ?? "service";
        if (!ValidItemKinds.Contains(itemKind)) return (null, "INVALID_ITEM_KIND");

        var service = new BusinessService
        {
            BusinessId = businessId,
            ServiceType = request.ServiceType.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            PhotoUrl = request.PhotoUrl?.Trim(),
            ItemKind = itemKind
        };

        var created = await serviceRepository.CreateAsync(service);
        return (MapResponse(created), null);
    }

    public async Task<IReadOnlyCollection<BusinessServiceResponse>> GetByBusinessIdAsync(Guid businessId) =>
        (await serviceRepository.GetByBusinessIdAsync(businessId))
            .Select(MapResponse)
            .ToList();

    public async Task<(BusinessServiceResponse? Service, string? ErrorCode)> UpdateAsync(
        Guid userId, Guid serviceId, UpdateBusinessServiceRequest request)
    {
        var service = await serviceRepository.GetByIdAsync(serviceId);
        if (service is null) return (null, "SERVICE_NOT_FOUND");

        var business = await businessRepository.GetByIdAsync(service.BusinessId);
        if (business is null || business.OwnerUserId != userId) return (null, "UNAUTHORIZED");

        if (request.Price < 0) return (null, "INVALID_PRICE");

        var itemKind = request.ItemKind?.Trim().ToLowerInvariant() ?? service.ItemKind;
        if (!ValidItemKinds.Contains(itemKind)) return (null, "INVALID_ITEM_KIND");

        service.ServiceType = request.ServiceType.Trim();
        service.Description = request.Description?.Trim();
        service.Price = request.Price;
        service.PhotoUrl = request.PhotoUrl?.Trim();
        service.ItemKind = itemKind;

        var updated = await serviceRepository.UpdateAsync(service);
        return (MapResponse(updated!), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid serviceId)
    {
        var service = await serviceRepository.GetByIdAsync(serviceId);
        if (service is null) return (false, "SERVICE_NOT_FOUND");

        var business = await businessRepository.GetByIdAsync(service.BusinessId);
        if (business is null || business.OwnerUserId != userId) return (false, "UNAUTHORIZED");

        await serviceRepository.DeleteAsync(serviceId);
        return (true, null);
    }

    private static BusinessServiceResponse MapResponse(BusinessService s) =>
        new(s.Id, s.BusinessId, s.ServiceType, s.Description, s.Price, s.PhotoUrl, s.CreatedAt, s.ItemKind);
}