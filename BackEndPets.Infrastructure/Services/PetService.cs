using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class PetService(IPetRepository petRepository) : IPetService
{
    public async Task<PetResponse> CreateAsync(Guid userId, CreatePetRequest request)
    {
        var pet = new Pet
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Species = request.Species?.Trim(),
            Breed = request.Breed?.Trim(),
            BirthDate = request.BirthDate,
            Description = request.Description?.Trim()
        };

        var created = await petRepository.CreateAsync(pet);
        return MapResponse(created);
    }

    public async Task<PetResponse?> GetByIdAsync(Guid id)
    {
        var pet = await petRepository.GetByIdAsync(id);
        return pet is null ? null : MapResponse(pet);
    }

    public async Task<IReadOnlyCollection<PetResponse>> GetByUserIdAsync(Guid userId) =>
        (await petRepository.GetByUserIdAsync(userId))
            .Select(MapResponse)
            .ToList();

    public async Task<(PetResponse? Pet, string? ErrorCode)> UpdateAsync(Guid userId, Guid petId, UpdatePetRequest request)
    {
        var existing = await petRepository.GetByIdAsync(petId);
        if (existing is null) return (null, "PET_NOT_FOUND");
        if (existing.UserId != userId) return (null, "UNAUTHORIZED");

        existing.Name = request.Name.Trim();
        existing.Species = request.Species?.Trim();
        existing.Breed = request.Breed?.Trim();
        existing.BirthDate = request.BirthDate;
        existing.Description = request.Description?.Trim();

        var updated = await petRepository.UpdateAsync(existing);
        return (MapResponse(updated!), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid petId)
    {
        var existing = await petRepository.GetByIdAsync(petId);
        if (existing is null) return (false, "PET_NOT_FOUND");
        if (existing.UserId != userId) return (false, "UNAUTHORIZED");

        await petRepository.DeleteAsync(petId);
        return (true, null);
    }

    private static PetResponse MapResponse(Pet p) =>
        new(p.Id, p.UserId, p.Name, p.Species, p.Breed, p.BirthDate, p.Description, p.PhotoUrl, p.CreatedAt);
}