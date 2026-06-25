namespace BackEndPets.Application.DTOs.Pets;

public sealed record CreatePetHistoryRequest(
    string Title,
    string? Description = null,
    string? DocumentUrl = null);

public sealed record PetHistoryResponse(
    Guid Id,
    Guid PetId,
    string Title,
    string? Description,
    string? DocumentUrl,
    DateTime CreatedAt);