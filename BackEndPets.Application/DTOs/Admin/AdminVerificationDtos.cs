namespace BackEndPets.Application.DTOs.Admin;

public sealed record UpdateVerificationStatusRequest(
    string VerificationStatus);

public sealed record AdminBusinessDocumentResponse(
    Guid Id,
    string DocumentType,
    string FileUrl,
    DateTime CreatedAt);

public sealed record AdminBusinessVerificationResponse(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string Nit,
    string Email,
    long Phone,
    string? Type,
    string? Location,
    string? Address,
    string VerificationStatus,
    DateTime CreatedAt,
    IReadOnlyCollection<AdminBusinessDocumentResponse> Documents);

public sealed record AdminWalkerDocumentResponse(
    Guid Id,
    string DocumentType,
    string FileUrl,
    DateTime CreatedAt);

public sealed record AdminWalkerVerificationResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string? ProfilePhoto,
    bool Available,
    string? WorkLocation,
    string? Experience,
    string? Description,
    string VerificationStatus,
    string? DocumentName,
    string? DocumentNumber,
    bool IsAcceptingBookings,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<AdminWalkerDocumentResponse> Documents);
