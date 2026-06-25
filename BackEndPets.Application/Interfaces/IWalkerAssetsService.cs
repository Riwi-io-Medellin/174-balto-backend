using BackEndPets.Application.DTOs.Walkers;

namespace BackEndPets.Application.Interfaces;

public interface IWalkerAssetsService
{
    Task<(WalkerGalleryResponse? Photo, string? ErrorCode)> AddPhotoAsync(
        Guid userId, AddWalkerPhotoRequest request);
    Task<(bool Success, string? ErrorCode)> DeletePhotoAsync(Guid userId, Guid photoId);
    Task<IReadOnlyCollection<WalkerGalleryResponse>> GetGalleryAsync(Guid walkerId);

    Task<(WalkerDocumentResponse? Document, string? ErrorCode)> AddDocumentAsync(
        Guid userId, AddWalkerDocumentRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteDocumentAsync(Guid userId, Guid documentId);
    Task<IReadOnlyCollection<WalkerDocumentResponse>> GetDocumentsAsync(Guid walkerId);
}