using BackEndPets.Domain.Entities;

namespace BackEndPets.Application.Interfaces;

public interface IPetClinicalDocumentGenerationService
{
    /// <summary>
    /// Builds the official Balto-format clinical history PDF from data already
    /// persisted in the database and uploads it, returning the public URL.
    /// </summary>
    Task<string> GenerateAsync(
        Pet pet,
        PetClinicalRecord record,
        IReadOnlyCollection<PetClinicalEvent> events,
        string ownerFullName,
        string? ownerPhone,
        string? ownerAddress,
        string? ownerEmail);
}
