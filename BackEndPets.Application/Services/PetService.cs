using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Application.Services;

public sealed class PetService(ICrudRepository<Pet> petRepository) : IPetService
{
    public async Task<IReadOnlyCollection<PetResponse>> GetByUserIdAsync(Guid userId)
    {
        var pets = await petRepository.GetAllAsync();
        return pets.Where(pet => pet.UserId == userId).Select(ToResponse).ToList();
    }

    public async Task<PetResponse?> GetByIdAsync(Guid userId, Guid id)
    {
        var pet = await petRepository.GetByIdAsync(id);
        return pet is null || pet.UserId != userId ? null : ToResponse(pet);
    }

    public async Task<PetResponse?> CreateAsync(Guid userId, CreatePetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Species))
        {
            return null;
        }

        var pet = new Pet
        {
            UserId = userId,
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

    public async Task<PetResponse?> UpdateAsync(Guid userId, Guid id, UpdatePetRequest request)
    {
        var pet = await petRepository.GetByIdAsync(id);
        if (pet is null || pet.UserId != userId || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Species))
        {
            return null;
        }

        pet.Name = request.Name.Trim();
        pet.Species = request.Species.Trim();
        pet.Breed = string.IsNullOrWhiteSpace(request.Breed) ? null : request.Breed.Trim();
        pet.BirthDate = request.BirthDate;
        pet.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        pet.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();

        await petRepository.UpdateAsync(pet);
        return ToResponse(pet);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var pet = await petRepository.GetByIdAsync(id);
        return pet is not null && pet.UserId == userId && await petRepository.DeleteAsync(id);
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
