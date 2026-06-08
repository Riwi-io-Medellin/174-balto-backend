using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Application.Services;

public sealed class PetService(
    ICrudRepository<Pet> petRepository,
    ICrudRepository<User> userRepository) : IPetService
{
    public async Task<IReadOnlyCollection<PetResponse>> GetAllAsync()
    {
        var pets = await petRepository.GetAllAsync();
        return pets.Select(ToResponse).ToList();
    }

    public async Task<PetResponse?> GetByIdAsync(Guid id)
    {
        var pet = await petRepository.GetByIdAsync(id);
        return pet is null ? null : ToResponse(pet);
    }

    public async Task<PetResponse?> CreateAsync(CreatePetRequest request)
    {
        if (!IsValid(request) || !await UserExistsAsync(request.UserId))
        {
            return null;
        }

        var pet = new Pet
        {
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Species = request.Species.Trim(),
            Breed = string.IsNullOrWhiteSpace(request.Breed) ? null : request.Breed.Trim(),
            BirthDate = request.BirthDate,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim()
        };

        await petRepository.AddAsync(pet);
        return ToResponse(pet);
    }

    public async Task<PetResponse?> UpdateAsync(Guid id, UpdatePetRequest request)
    {
        var pet = await petRepository.GetByIdAsync(id);
        if (pet is null || !IsValid(request) || !await UserExistsAsync(request.UserId))
        {
            return null;
        }

        pet.UserId = request.UserId;
        pet.Name = request.Name.Trim();
        pet.Species = request.Species.Trim();
        pet.Breed = string.IsNullOrWhiteSpace(request.Breed) ? null : request.Breed.Trim();
        pet.BirthDate = request.BirthDate;
        pet.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        pet.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();

        await petRepository.UpdateAsync(pet);
        return ToResponse(pet);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await petRepository.DeleteAsync(id);
    }

    private static bool IsValid(CreatePetRequest request)
    {
        return IsValid(request.UserId, request.Name, request.Species);
    }

    private static bool IsValid(UpdatePetRequest request)
    {
        return IsValid(request.UserId, request.Name, request.Species);
    }

    private static bool IsValid(Guid userId, string name, string species)
    {
        return userId != Guid.Empty
            && !string.IsNullOrWhiteSpace(name)
            && !string.IsNullOrWhiteSpace(species);
    }

    private async Task<bool> UserExistsAsync(Guid userId)
    {
        return await userRepository.GetByIdAsync(userId) is not null;
    }

    private static PetResponse ToResponse(Pet pet) => new(
        pet.Id,
        pet.UserId,
        pet.Name,
        pet.Species,
        pet.Breed,
        pet.BirthDate,
        pet.Description,
        pet.PhotoUrl,
        pet.CreatedAt);
}
