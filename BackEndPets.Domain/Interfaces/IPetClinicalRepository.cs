using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IPetClinicalRepository
{
    Task<PetClinicalRecord?> GetRecordByPetIdAsync(Guid petId);
    Task<PetClinicalRecord> CreateRecordAsync(PetClinicalRecord record);
    Task<PetClinicalRecord> UpdateRecordAsync(PetClinicalRecord record);

    Task<PetClinicalEvent> CreateEventAsync(PetClinicalEvent clinicalEvent);
    Task<IReadOnlyCollection<PetClinicalEvent>> GetEventsByPetIdAsync(Guid petId);

    Task<PetClinicalDocument> CreateDocumentAsync(PetClinicalDocument document);
    Task<PetClinicalDocument?> GetDocumentByIdAsync(Guid id);
    Task<PetClinicalDocument> UpdateDocumentAsync(PetClinicalDocument document);

    Task ReplaceTipsAsync(Guid petId, IReadOnlyCollection<PetClinicalTip> tips);
    Task<IReadOnlyCollection<PetClinicalTip>> GetTipsByPetIdAsync(Guid petId);

    Task<PetVetDocumentAnalysis> CreateVetDocumentAnalysisAsync(PetVetDocumentAnalysis analysis);
    Task<IReadOnlyCollection<PetVetDocumentAnalysis>> GetVetDocumentAnalysesByPetIdAsync(Guid petId, int take = 5);
    Task<IReadOnlyDictionary<Guid, PetVetDocumentAnalysis>> GetLatestVetDocumentAnalysesByPetIdsAsync(IReadOnlyCollection<Guid> petIds);
}
