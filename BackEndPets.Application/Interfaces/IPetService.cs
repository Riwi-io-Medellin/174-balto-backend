using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetService
{
    Task<PetResponse> CreateAsync(Guid userId, CreatePetRequest request);
    Task<PetResponse?> GetByIdAsync(Guid id);
    Task<PagedResult<PetResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<(PetResponse? Pet, string? ErrorCode)> UpdateAsync(Guid userId, Guid petId, UpdatePetRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid petId);
    Task<(PetResponse? Pet, string? ErrorCode)> ReportLostAsync(Guid userId, Guid petId, ReportPetLostRequest request);
    Task<(PetResponse? Pet, string? ErrorCode)> MarkFoundAsync(Guid userId, Guid petId);
    Task<PublicPetTagResponse?> GetPublicTagInfoAsync(Guid petId);
}
