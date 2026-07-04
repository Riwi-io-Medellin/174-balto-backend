using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetClinicalExtractionService
{
    /// <summary>
    /// Sends the uploaded document images to the vision model and extracts
    /// pet profile, clinical-record and clinical-event fields in one pass
    /// (OCR + extraction combined, no separate OCR step).
    /// </summary>
    Task<(ClinicalExtractionDraftResponse? Draft, string? ErrorCode)> ExtractAsync(
        IReadOnlyCollection<string> fileUrls);
}
