namespace BackEndPets.Application.DTOs.Pets;

public sealed record CreatePetRequest(
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description);

public sealed record UpdatePetRequest(
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description);

public sealed record PetResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string? Species,
    string? Breed,
    DateTime? BirthDate,
    string? Description,
    string? PhotoUrl,
    DateTime CreatedAt);