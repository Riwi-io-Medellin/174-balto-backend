using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Services;

public sealed class PetClinicalRecordService(
    IPetRepository petRepository,
    IPetClinicalRepository clinicalRepository,
    IPetClinicalExtractionService extractionService,
    IPetClinicalDocumentGenerationService documentGenerationService,
    IPetClinicalTipsService tipsService,
    UserManager<ApplicationUser> userManager) : IPetClinicalRecordService
{
    public async Task<(Guid? DocumentId, string? ErrorCode)> UploadSourceDocumentAsync(
        Guid userId, Guid petId, string fileUrl, string fileName, string fileType)
    {
        var (_, errorCode) = await AuthorizeAsync(userId, petId);
        if (errorCode is not null) return (null, errorCode);

        var document = await clinicalRepository.CreateDocumentAsync(new PetClinicalDocument
        {
            PetId = petId,
            FileUrl = fileUrl,
            FileName = fileName,
            FileType = fileType,
            Status = PetClinicalDocumentStatus.Pending
        });

        return (document.Id, null);
    }

    public async Task<(ClinicalExtractionDraftResponse? Draft, string? ErrorCode)> RunExtractionAsync(
        Guid userId, Guid petId, IReadOnlyCollection<Guid> documentIds)
    {
        var (_, authError) = await AuthorizeAsync(userId, petId);
        if (authError is not null) return (null, authError);

        var documents = new List<PetClinicalDocument>();
        foreach (var id in documentIds)
        {
            var d = await clinicalRepository.GetDocumentByIdAsync(id);
            if (d is null || d.PetId != petId) return (null, "DOCUMENT_NOT_FOUND");
            documents.Add(d);
        }

        var (draft, errorCode) = await extractionService.ExtractAsync(
            documents.Select(d => d.FileUrl).ToList());

        foreach (var d in documents)
        {
            d.Status = errorCode is null ? PetClinicalDocumentStatus.Processed : PetClinicalDocumentStatus.Failed;
            d.ErrorMessage = errorCode;
            d.ProcessedAt = DateTime.UtcNow;
            await clinicalRepository.UpdateDocumentAsync(d);
        }

        if (errorCode is not null) return (null, errorCode);
        return (draft, null);
    }

    public async Task<(ClinicalEventResponse? Event, string? ErrorCode)> ConfirmEventAsync(
        Guid userId, Guid petId, ConfirmClinicalEventRequest request)
    {
        var (pet, authError) = await AuthorizeAsync(userId, petId);
        if (authError is not null || pet is null) return (null, authError);

        // 1. Apply reviewed pet-profile fields (only overwrite what the user provided).
        var profile = request.PetProfile;
        pet.Sex = profile.Sex ?? pet.Sex;
        pet.Color = profile.Color ?? pet.Color;
        pet.IdentificationNumber = profile.IdentificationNumber ?? pet.IdentificationNumber;
        pet.MicrochipNumber = profile.MicrochipNumber ?? pet.MicrochipNumber;
        pet.Weight = profile.Weight ?? pet.Weight;
        await petRepository.UpdateAsync(pet);

        // 2. Get-or-create the clinical record (cumulative/current-state data).
        var record = await clinicalRepository.GetRecordByPetIdAsync(petId);
        var recordDraft = request.ClinicalRecord;
        if (record is null)
        {
            record = await clinicalRepository.CreateRecordAsync(new PetClinicalRecord
            {
                PetId = petId,
                Allergies = recordDraft.Allergies,
                ChronicConditions = recordDraft.ChronicConditions,
                DietaryRestrictions = recordDraft.DietaryRestrictions
            });
        }
        else
        {
            record.Allergies = recordDraft.Allergies ?? record.Allergies;
            record.ChronicConditions = recordDraft.ChronicConditions ?? record.ChronicConditions;
            record.DietaryRestrictions = recordDraft.DietaryRestrictions ?? record.DietaryRestrictions;
            record = await clinicalRepository.UpdateRecordAsync(record);
        }

        // 3. Persist the new clinical event — history grows, never overwrites.
        var eventDraft = request.Event;
        var clinicalEvent = new PetClinicalEvent
        {
            PetId = petId,
            EventType = eventDraft.EventType,
            EventDate = eventDraft.EventDate,
            ClinicName = eventDraft.ClinicName,
            VeterinarianName = eventDraft.VeterinarianName,
            Reason = eventDraft.Reason,
            ClinicalSigns = eventDraft.ClinicalSigns,
            Temperature = eventDraft.Temperature,
            HeartRate = eventDraft.HeartRate,
            RespiratoryRate = eventDraft.RespiratoryRate,
            Weight = eventDraft.Weight,
            BodyCondition = eventDraft.BodyCondition,
            Findings = eventDraft.Findings,
            Diagnosis = eventDraft.Diagnosis,
            ExamsPerformed = eventDraft.ExamsPerformed,
            ExamResults = eventDraft.ExamResults,
            Procedures = eventDraft.Procedures,
            Recommendations = eventDraft.Recommendations,
            Observations = eventDraft.Observations,
            NextControlDate = eventDraft.NextControlDate,
            Source = PetClinicalEventSource.Manual,
            Medications = eventDraft.Medications.Select(m => new PetClinicalMedication
            {
                Name = m.Name,
                Dose = m.Dose,
                Frequency = m.Frequency,
                Duration = m.Duration
            }).ToList()
        };
        var createdEvent = await clinicalRepository.CreateEventAsync(clinicalEvent);

        // 4. Regenerate the official document from DB data only.
        var allEvents = await clinicalRepository.GetEventsByPetIdAsync(petId);
        var owner = await userManager.FindByIdAsync(pet.UserId.ToString());
        var ownerFullName = owner is null ? "N/A" : $"{owner.FirstName} {owner.LastName}".Trim();

        var documentUrl = await documentGenerationService.GenerateAsync(
            pet, record, allEvents, ownerFullName, owner?.Phone, owner?.Address, owner?.Email);

        record.DocumentUrl = documentUrl;
        record.DocumentGeneratedAt = DateTime.UtcNow;
        record = await clinicalRepository.UpdateRecordAsync(record);

        // 5. Regenerate tips only because the history just changed.
        var tips = await tipsService.GenerateAsync(pet, record, allEvents);
        await clinicalRepository.ReplaceTipsAsync(petId, tips);

        return (MapEvent(createdEvent), null);
    }

    public async Task<(ClinicalRecordResponse? Record, string? ErrorCode)> GetRecordAsync(Guid userId, Guid petId)
    {
        var (_, authError) = await AuthorizeAsync(userId, petId);
        if (authError is not null) return (null, authError);

        var record = await clinicalRepository.GetRecordByPetIdAsync(petId);
        var events = await clinicalRepository.GetEventsByPetIdAsync(petId);

        var response = new ClinicalRecordResponse(
            petId,
            record?.Allergies,
            record?.ChronicConditions,
            record?.DietaryRestrictions,
            record?.DocumentUrl,
            record?.DocumentGeneratedAt,
            events.Select(MapEvent).ToList());

        return (response, null);
    }

    public async Task<(IReadOnlyCollection<ClinicalTipResponse>? Tips, string? ErrorCode)> GetTipsAsync(Guid userId, Guid petId)
    {
        var (_, authError) = await AuthorizeAsync(userId, petId);
        if (authError is not null) return (null, authError);

        var tips = await clinicalRepository.GetTipsByPetIdAsync(petId);
        return (tips.Select(t => new ClinicalTipResponse(t.Id, t.Category, t.Message, t.CreatedAt)).ToList(), null);
    }

    private async Task<(Pet? Pet, string? ErrorCode)> AuthorizeAsync(Guid userId, Guid petId)
    {
        var pet = await petRepository.GetByIdAsync(petId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != userId) return (null, "UNAUTHORIZED");
        return (pet, null);
    }

    private static ClinicalEventResponse MapEvent(PetClinicalEvent e) => new(
        e.Id, e.PetId, e.EventType, e.EventDate, e.ClinicName, e.VeterinarianName, e.Reason,
        e.ClinicalSigns, e.Temperature, e.HeartRate, e.RespiratoryRate, e.Weight, e.BodyCondition,
        e.Findings, e.Diagnosis, e.ExamsPerformed, e.ExamResults, e.Procedures, e.Recommendations,
        e.Observations, e.NextControlDate, e.Source, e.CreatedAt,
        e.Medications.Select(m => new ClinicalMedicationResponse(m.Id, m.Name, m.Dose, m.Frequency, m.Duration)).ToList());
}
