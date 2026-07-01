namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record AddHomeProviderPhotoRequest(string PhotoUrl);

public sealed record HomeProviderGalleryResponse(
    Guid Id,
    Guid ProviderId,
    string PhotoUrl,
    DateTime CreatedAt);

public sealed record AddHomeProviderDocumentRequest(
    string DocumentType,
    string FileUrl);

public sealed record HomeProviderDocumentResponse(
    Guid Id,
    Guid ProviderId,
    string DocumentType,
    string FileUrl,
    DateTime CreatedAt);

public sealed record AddHomeProviderCertificationRequest(
    Guid? ServiceTypeId,
    string Title,
    string? IssuingOrganization,
    string? CredentialNumber,
    DateOnly? IssuedDate,
    DateOnly? ExpiryDate,
    string? DocumentUrl);

public sealed record HomeProviderCertificationResponse(
    Guid Id,
    Guid ProviderId,
    Guid? ServiceTypeId,
    string Title,
    string? IssuingOrganization,
    string? CredentialNumber,
    DateOnly? IssuedDate,
    DateOnly? ExpiryDate,
    string? DocumentUrl,
    DateTime CreatedAt);

public sealed record AddHomeProviderSpecialtyRequest(string Specialty);

public sealed record HomeProviderSpecialtyResponse(
    Guid Id,
    Guid ProviderId,
    string Specialty,
    DateTime CreatedAt);
