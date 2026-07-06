using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Tests.Fakes;

public sealed class FakePetClinicalRepository : IPetClinicalRepository
{
    public List<PetVetDocumentAnalysis> CreatedAnalyses { get; } = [];

    public Task<PetVetDocumentAnalysis> CreateVetDocumentAnalysisAsync(PetVetDocumentAnalysis analysis)
    {
        analysis.Id = Guid.NewGuid();
        analysis.CreatedAt = DateTime.UtcNow;
        CreatedAnalyses.Add(analysis);
        return Task.FromResult(analysis);
    }

    public Task<IReadOnlyCollection<PetVetDocumentAnalysis>> GetVetDocumentAnalysesByPetIdAsync(Guid petId, int take = 5) =>
        Task.FromResult<IReadOnlyCollection<PetVetDocumentAnalysis>>(
            CreatedAnalyses.Where(a => a.PetId == petId).Take(take).ToList());

    public Task<PetClinicalRecord?> GetRecordByPetIdAsync(Guid petId) =>
        throw new NotImplementedException();

    public Task<PetClinicalRecord> CreateRecordAsync(PetClinicalRecord record) =>
        throw new NotImplementedException();

    public Task<PetClinicalRecord> UpdateRecordAsync(PetClinicalRecord record) =>
        throw new NotImplementedException();

    public Task<PetClinicalEvent> CreateEventAsync(PetClinicalEvent clinicalEvent) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<PetClinicalEvent>> GetEventsByPetIdAsync(Guid petId) =>
        throw new NotImplementedException();

    public Task<PetClinicalDocument> CreateDocumentAsync(PetClinicalDocument document) =>
        throw new NotImplementedException();

    public Task<PetClinicalDocument?> GetDocumentByIdAsync(Guid id) =>
        throw new NotImplementedException();

    public Task<PetClinicalDocument> UpdateDocumentAsync(PetClinicalDocument document) =>
        throw new NotImplementedException();

    public Task ReplaceTipsAsync(Guid petId, IReadOnlyCollection<PetClinicalTip> tips) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<PetClinicalTip>> GetTipsByPetIdAsync(Guid petId) =>
        throw new NotImplementedException();
}
