namespace BackEndPets.Application.DTOs.Walkers;

public sealed record AddWalkerPhotoRequest(string PhotoUrl);

public sealed record WalkerGalleryResponse(
    Guid Id,
    Guid WalkerId,
    string PhotoUrl,
    DateTime CreatedAt);

public sealed record AddWalkerDocumentRequest(
    string DocumentType,
    string FileUrl);

public sealed record WalkerDocumentResponse(
    Guid Id,
    Guid WalkerId,
    string DocumentType,
    string FileUrl,
    DateTime CreatedAt);