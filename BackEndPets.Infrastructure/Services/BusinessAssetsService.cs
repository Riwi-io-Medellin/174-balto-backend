using BackEndPets.Application.DTOs.Businesses;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class BusinessAssetsService(
    IBusinessRepository businessRepository,
    IBusinessDocumentRepository documentRepository) : IBusinessAssetsService
{
    public async Task<(BusinessDocumentResponse? Document, string? ErrorCode)> AddDocumentAsync(
        Guid userId, Guid businessId, AddBusinessDocumentRequest request)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business is null) return (null, "BUSINESS_NOT_FOUND");
        if (business.OwnerUserId != userId) return (null, "UNAUTHORIZED");

        var document = new BusinessDocument
        {
            BusinessId = businessId,
            DocumentType = request.DocumentType.Trim(),
            FileUrl = request.FileUrl.Trim()
        };

        var created = await documentRepository.CreateAsync(document);
        return (MapResponse(created), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteDocumentAsync(Guid userId, Guid documentId)
    {
        var document = await documentRepository.GetByIdAsync(documentId);
        if (document is null) return (false, "DOCUMENT_NOT_FOUND");

        var business = await businessRepository.GetByIdAsync(document.BusinessId);
        if (business is null || business.OwnerUserId != userId) return (false, "UNAUTHORIZED");

        await documentRepository.DeleteAsync(documentId);
        return (true, null);
    }

    public async Task<IReadOnlyCollection<BusinessDocumentResponse>> GetDocumentsAsync(Guid businessId) =>
        (await documentRepository.GetByBusinessIdAsync(businessId))
            .Select(MapResponse)
            .ToList();

    private static BusinessDocumentResponse MapResponse(BusinessDocument d) =>
        new(d.Id, d.BusinessId, d.DocumentType, d.FileUrl, d.CreatedAt);
}