using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;

namespace BackEndPets.Application.Interfaces;

public interface IPetClinicalRecordService
{
    Task<(Guid? DocumentId, string? ErrorCode)> UploadSourceDocumentAsync(
        Guid userId, Guid petId, string fileUrl, string fileName, string fileType);

    Task<(ClinicalExtractionDraftResponse? Draft, string? ErrorCode)> RunExtractionAsync(
        Guid userId, Guid petId, IReadOnlyCollection<Guid> documentIds);

    Task<(ClinicalEventResponse? Event, string? ErrorCode)> ConfirmEventAsync(
        Guid userId, Guid petId, ConfirmClinicalEventRequest request);

    Task<(ClinicalRecordResponse? Record, string? ErrorCode)> GetRecordAsync(Guid userId, Guid petId);

    Task<(IReadOnlyCollection<ClinicalTipResponse>? Tips, string? ErrorCode)> GetTipsAsync(Guid userId, Guid petId);
}
