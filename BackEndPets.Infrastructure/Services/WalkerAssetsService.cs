using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkerAssetsService(
    IWalkerRepository walkerRepository,
    IWalkerGalleryRepository galleryRepository,
    IWalkerDocumentRepository documentRepository) : IWalkerAssetsService
{
    public async Task<(WalkerGalleryResponse? Photo, string? ErrorCode)> AddPhotoAsync(
        Guid userId, AddWalkerPhotoRequest request)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var photo = new WalkerGallery
        {
            WalkerId = walker.Id,
            PhotoUrl = request.PhotoUrl.Trim()
        };

        var created = await galleryRepository.CreateAsync(photo);
        return (MapPhoto(created), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeletePhotoAsync(Guid userId, Guid photoId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (false, "WALKER_NOT_FOUND");

        var photo = await galleryRepository.GetByIdAsync(photoId);
        if (photo is null) return (false, "PHOTO_NOT_FOUND");
        if (photo.WalkerId != walker.Id) return (false, "UNAUTHORIZED");

        await galleryRepository.DeleteAsync(photoId);
        return (true, null);
    }

    public async Task<IReadOnlyCollection<WalkerGalleryResponse>> GetGalleryAsync(Guid walkerId) =>
        (await galleryRepository.GetByWalkerIdAsync(walkerId))
            .Select(MapPhoto)
            .ToList();

    public async Task<(WalkerDocumentResponse? Document, string? ErrorCode)> AddDocumentAsync(
        Guid userId, AddWalkerDocumentRequest request)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var document = new WalkerDocument
        {
            WalkerId = walker.Id,
            DocumentType = request.DocumentType.Trim(),
            FileUrl = request.FileUrl.Trim()
        };

        var created = await documentRepository.CreateAsync(document);
        return (MapDocument(created), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteDocumentAsync(Guid userId, Guid documentId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return (false, "WALKER_NOT_FOUND");

        var document = await documentRepository.GetByIdAsync(documentId);
        if (document is null) return (false, "DOCUMENT_NOT_FOUND");
        if (document.WalkerId != walker.Id) return (false, "UNAUTHORIZED");

        await documentRepository.DeleteAsync(documentId);
        return (true, null);
    }

    public async Task<IReadOnlyCollection<WalkerDocumentResponse>> GetDocumentsAsync(Guid walkerId) =>
        (await documentRepository.GetByWalkerIdAsync(walkerId))
            .Select(MapDocument)
            .ToList();

    private static WalkerGalleryResponse MapPhoto(WalkerGallery g) =>
        new(g.Id, g.WalkerId, g.PhotoUrl, g.CreatedAt);

    private static WalkerDocumentResponse MapDocument(WalkerDocument d) =>
        new(d.Id, d.WalkerId, d.DocumentType, d.FileUrl, d.CreatedAt);
}