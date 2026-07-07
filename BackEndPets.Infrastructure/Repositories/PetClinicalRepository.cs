using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class PetClinicalRepository(AppIdentityDbContext dbContext) : IPetClinicalRepository
{
    public Task<PetClinicalRecord?> GetRecordByPetIdAsync(Guid petId) =>
        dbContext.PetClinicalRecords.FirstOrDefaultAsync(r => r.PetId == petId);

    public async Task<PetClinicalRecord> CreateRecordAsync(PetClinicalRecord record)
    {
        record.Id = Guid.NewGuid();
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;
        dbContext.PetClinicalRecords.Add(record);
        await dbContext.SaveChangesAsync();
        return record;
    }

    public async Task<PetClinicalRecord> UpdateRecordAsync(PetClinicalRecord record)
    {
        var existing = await dbContext.PetClinicalRecords.FirstAsync(r => r.Id == record.Id);
        existing.Allergies = record.Allergies;
        existing.ChronicConditions = record.ChronicConditions;
        existing.DietaryRestrictions = record.DietaryRestrictions;
        existing.DocumentUrl = record.DocumentUrl;
        existing.DocumentGeneratedAt = record.DocumentGeneratedAt;
        existing.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<PetClinicalEvent> CreateEventAsync(PetClinicalEvent clinicalEvent)
    {
        clinicalEvent.Id = Guid.NewGuid();
        clinicalEvent.CreatedAt = DateTime.UtcNow;
        foreach (var med in clinicalEvent.Medications)
        {
            med.Id = Guid.NewGuid();
            med.ClinicalEventId = clinicalEvent.Id;
            med.CreatedAt = DateTime.UtcNow;
        }
        dbContext.PetClinicalEvents.Add(clinicalEvent);
        await dbContext.SaveChangesAsync();
        return clinicalEvent;
    }

    public async Task<IReadOnlyCollection<PetClinicalEvent>> GetEventsByPetIdAsync(Guid petId) =>
        await dbContext.PetClinicalEvents
            .Include(e => e.Medications)
            .Where(e => e.PetId == petId)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();

    public async Task<PetClinicalDocument> CreateDocumentAsync(PetClinicalDocument document)
    {
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        dbContext.PetClinicalDocuments.Add(document);
        await dbContext.SaveChangesAsync();
        return document;
    }

    public Task<PetClinicalDocument?> GetDocumentByIdAsync(Guid id) =>
        dbContext.PetClinicalDocuments.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<PetClinicalDocument> UpdateDocumentAsync(PetClinicalDocument document)
    {
        var existing = await dbContext.PetClinicalDocuments.FirstAsync(d => d.Id == document.Id);
        existing.Status = document.Status;
        existing.ExtractedJson = document.ExtractedJson;
        existing.ErrorMessage = document.ErrorMessage;
        existing.ProcessedAt = document.ProcessedAt;
        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task ReplaceTipsAsync(Guid petId, IReadOnlyCollection<PetClinicalTip> tips)
    {
        var existing = await dbContext.PetClinicalTips.Where(t => t.PetId == petId).ToListAsync();
        dbContext.PetClinicalTips.RemoveRange(existing);

        foreach (var tip in tips)
        {
            tip.Id = Guid.NewGuid();
            tip.PetId = petId;
            tip.CreatedAt = DateTime.UtcNow;
        }
        dbContext.PetClinicalTips.AddRange(tips);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<PetClinicalTip>> GetTipsByPetIdAsync(Guid petId) =>
        await dbContext.PetClinicalTips
            .Where(t => t.PetId == petId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<PetVetDocumentAnalysis> CreateVetDocumentAnalysisAsync(PetVetDocumentAnalysis analysis)
    {
        analysis.Id = Guid.NewGuid();
        analysis.CreatedAt = DateTime.UtcNow;
        dbContext.PetVetDocumentAnalyses.Add(analysis);
        await dbContext.SaveChangesAsync();
        return analysis;
    }

    public async Task<IReadOnlyCollection<PetVetDocumentAnalysis>> GetVetDocumentAnalysesByPetIdAsync(Guid petId, int take = 5) =>
        await dbContext.PetVetDocumentAnalyses
            .Where(a => a.PetId == petId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync();

    public async Task<IReadOnlyDictionary<Guid, PetVetDocumentAnalysis>> GetLatestVetDocumentAnalysesByPetIdsAsync(
        IReadOnlyCollection<Guid> petIds)
    {
        if (petIds.Count == 0) return new Dictionary<Guid, PetVetDocumentAnalysis>();

        var all = await dbContext.PetVetDocumentAnalyses
            .Where(a => petIds.Contains(a.PetId))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return all
            .GroupBy(a => a.PetId)
            .ToDictionary(g => g.Key, g => g.First());
    }
}
