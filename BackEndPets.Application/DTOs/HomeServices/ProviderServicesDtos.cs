namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record AddHomeProviderServiceRequest(
    Guid ServiceTypeId,
    decimal? Price,
    string PriceUnit,
    string? Description = null);

public sealed record UpdateHomeProviderServiceRequest(
    decimal? Price,
    string PriceUnit,
    string? Description,
    bool IsActive);

public sealed record HomeProviderServiceResponse(
    Guid Id,
    Guid ProviderId,
    Guid ServiceTypeId,
    string ServiceTypeCode,
    string ServiceTypeName,
    decimal? Price,
    string PriceUnit,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);
