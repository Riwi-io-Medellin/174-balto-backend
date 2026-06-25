namespace BackEndPets.Application.DTOs.Businesses;

public sealed record AddBusinessDocumentRequest(
    string DocumentType,
    string FileUrl);

public sealed record BusinessDocumentResponse(
    Guid Id,
    Guid BusinessId,
    string DocumentType,
    string FileUrl,
    DateTime CreatedAt);