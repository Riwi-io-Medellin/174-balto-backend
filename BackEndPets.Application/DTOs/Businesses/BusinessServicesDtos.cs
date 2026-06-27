namespace BackEndPets.Application.DTOs.Businesses;

public sealed record CreateBusinessServiceRequest(
    string ServiceType,
    decimal Price,
    string? Description = null,
    string? PhotoUrl = null);

public sealed record UpdateBusinessServiceRequest(
    string ServiceType,
    decimal Price,
    string? Description = null,
    string? PhotoUrl = null);

public sealed record BusinessServiceResponse(
    Guid Id,
    Guid BusinessId,
    string ServiceType,
    string? Description,
    decimal Price,
    string? PhotoUrl,
    DateTime CreatedAt);