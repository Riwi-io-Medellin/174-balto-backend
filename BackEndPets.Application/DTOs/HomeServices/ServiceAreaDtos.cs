namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record HomeProviderServiceAreaRequest(
    string? Label,
    double Latitude,
    double Longitude,
    decimal RadiusKm);

public sealed record HomeProviderServiceAreaResponse(
    Guid Id,
    Guid ProviderId,
    string? Label,
    double Latitude,
    double Longitude,
    decimal RadiusKm,
    DateTime CreatedAt);
