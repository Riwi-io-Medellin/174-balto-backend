using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetService
{
    Task<PetResponse> CreateAsync(Guid userId, CreatePetRequest request);
    Task<PetResponse?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<PetResponse>> GetByUserIdAsync(Guid userId);
    Task<(PetResponse? Pet, string? ErrorCode)> UpdateAsync(Guid userId, Guid petId, UpdatePetRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid petId);
}