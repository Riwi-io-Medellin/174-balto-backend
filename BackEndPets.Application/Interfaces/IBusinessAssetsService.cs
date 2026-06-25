using BackEndPets.Application.DTOs.Businesses;

namespace BackEndPets.Application.Interfaces;

public interface IBusinessAssetsService
{
    Task<(BusinessDocumentResponse? Document, string? ErrorCode)> AddDocumentAsync(
        Guid userId, Guid businessId, AddBusinessDocumentRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteDocumentAsync(Guid userId, Guid documentId);
    Task<IReadOnlyCollection<BusinessDocumentResponse>> GetDocumentsAsync(Guid businessId);
}