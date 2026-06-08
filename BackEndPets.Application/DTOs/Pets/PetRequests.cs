namespace BackEndPets.Application.DTOs.Pets;

public sealed record CreatePetRequest(
    Guid UserId,
    string Name,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    string? Description,
    string? PhotoUrl);

public sealed record UpdatePetRequest(
    Guid UserId,
    string Name,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    string? Description,
    string? PhotoUrl);

public sealed record PetResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    string? Description,
    string? PhotoUrl,
    DateTime CreatedAt);
