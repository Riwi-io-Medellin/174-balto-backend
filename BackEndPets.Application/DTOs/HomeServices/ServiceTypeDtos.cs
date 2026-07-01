namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record HomeServiceTypeResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description);
