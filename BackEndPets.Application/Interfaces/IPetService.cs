using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetService
{
    Task<IReadOnlyCollection<PetResponse>> GetByUserIdAsync(Guid userId);

    Task<PetResponse?> GetByIdAsync(Guid userId, Guid id);

    Task<PetResponse?> CreateAsync(Guid userId, CreatePetRequest request);

    Task<PetResponse?> UpdateAsync(Guid userId, Guid id, UpdatePetRequest request);

    Task<bool> DeleteAsync(Guid userId, Guid id);
}
