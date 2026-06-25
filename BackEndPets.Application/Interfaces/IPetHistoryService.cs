using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetHistoryService
{
    Task<(PetHistoryResponse? History, string? ErrorCode)> CreateAsync(
        Guid userId, Guid petId, CreatePetHistoryRequest request);
    Task<(IReadOnlyCollection<PetHistoryResponse>? Histories, string? ErrorCode)> GetByPetIdAsync(
        Guid userId, Guid petId);
    Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid historyId);
}