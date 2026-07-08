namespace BackEndPets.Application.DTOs.Pets;

public sealed record CreatePetRequest(
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description,
    decimal? Weight,
    string? PhotoUrl = null,
    string? Sex = null);

public sealed record UpdatePetRequest(
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description,
    decimal? Weight,
    string? PhotoUrl = null,
    string? Sex = null);

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
    DateTime? LostAt,
    string? LatestHealthUrgency = null,
    DateTime? LatestHealthAnalysisAt = null);
    
public sealed record ReportPetLostRequest(
    double LostLatitude,
    double LostLongitude);

public sealed record PublicPetTagResponse(
    Guid Id,
    string Name,
    string? Species,
    string? Breed,
    string? PhotoUrl,
    string? Sex,
    string? Color,
    decimal? Weight,
    DateTime? BirthDate,
    bool IsLost,
    string OwnerName,
    string OwnerPhone);

public sealed record ShareTagLocationRequest(
    double Latitude,
    double Longitude);