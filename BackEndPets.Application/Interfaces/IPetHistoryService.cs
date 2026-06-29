using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetHistoryService
{
    Task<(PetHistoryResponse? History, string? ErrorCode)> CreateAsync(
        Guid userId, Guid petId, CreatePetHistoryRequest request);
    Task<(PagedResult<PetHistoryResponse>? Result, string? ErrorCode)> GetByPetIdAsync(
        Guid userId, Guid petId, int page = 1, int pageSize = 20);
    Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid historyId);
}
