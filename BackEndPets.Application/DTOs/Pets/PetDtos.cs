namespace BackEndPets.Application.DTOs.Pets;

public sealed record CreatePetRequest(
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description,
    decimal? Weight,
    string? PhotoUrl = null);

public sealed record UpdatePetRequest(
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description,
    decimal? Weight,
    string? PhotoUrl = null);

public sealed record PetResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description,
    string? PhotoUrl,
    decimal? Weight,
    string? Sex,
    string? Color,
    string? IdentificationNumber,
    string? MicrochipNumber,
    DateTime CreatedAt,
    bool IsLost,
    double? LostLatitude,
    double? LostLongitude,
    DateTime? LostAt);
    
public sealed record ReportPetLostRequest(
    double LostLatitude,
    double LostLongitude);