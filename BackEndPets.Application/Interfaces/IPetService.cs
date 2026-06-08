using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetService
{
    Task<IReadOnlyCollection<PetResponse>> GetAllAsync();

    Task<PetResponse?> GetByIdAsync(Guid id);

    Task<PetResponse?> CreateAsync(CreatePetRequest request);

    Task<PetResponse?> UpdateAsync(Guid id, UpdatePetRequest request);

    Task<bool> DeleteAsync(Guid id);
}
