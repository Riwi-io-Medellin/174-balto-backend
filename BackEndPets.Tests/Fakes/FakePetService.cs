using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.Tests.Fakes;

public sealed class FakePetService : IPetService
{
    public PetResponse? PetToReturn { get; set; }

    public Task<PetResponse?> GetByIdAsync(Guid id) => Task.FromResult(PetToReturn);

    public Task<PetResponse> CreateAsync(Guid userId, CreatePetRequest request) =>
        throw new NotImplementedException();

    public Task<PagedResult<PetResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20) =>
        throw new NotImplementedException();

    public Task<(PetResponse? Pet, string? ErrorCode)> UpdateAsync(Guid userId, Guid petId, UpdatePetRequest request) =>
        throw new NotImplementedException();

    public Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid petId) =>
        throw new NotImplementedException();

    public Task<(PetResponse? Pet, string? ErrorCode)> ReportLostAsync(Guid userId, Guid petId, ReportPetLostRequest request) =>
        throw new NotImplementedException();

    public Task<(PetResponse? Pet, string? ErrorCode)> MarkFoundAsync(Guid userId, Guid petId) =>
        throw new NotImplementedException();

    public Task<PublicPetTagResponse?> GetPublicTagInfoAsync(Guid petId) =>
        throw new NotImplementedException();

    public Task<(bool Success, string? ErrorCode)> ShareTagLocationAsync(Guid petId, ShareTagLocationRequest request) =>
        throw new NotImplementedException();
}
