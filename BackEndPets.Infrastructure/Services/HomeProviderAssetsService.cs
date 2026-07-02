using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeProviderAssetsService(
    IHomeServiceProviderRepository providerRepository,
    IHomeProviderGalleryRepository galleryRepository,
    IHomeProviderDocumentRepository documentRepository,
    IHomeProviderCertificationRepository certificationRepository,
    IHomeProviderSpecialtyRepository specialtyRepository) : IHomeProviderAssetsService
{
    public async Task<(HomeProviderGalleryResponse? Photo, string? ErrorCode)> AddPhotoAsync(
        Guid userId, AddHomeProviderPhotoRequest request)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var photo = new HomeProviderGallery
        {
            ProviderId = provider.Id,
            PhotoUrl = request.PhotoUrl.Trim()
        };

        var created = await galleryRepository.CreateAsync(photo);
        return (MapPhoto(created), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeletePhotoAsync(Guid userId, Guid photoId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (false, "PROVIDER_NOT_FOUND");

        var photo = await galleryRepository.GetByIdAsync(photoId);
        if (photo is null) return (false, "PHOTO_NOT_FOUND");
        if (photo.ProviderId != provider.Id) return (false, "UNAUTHORIZED");

        await galleryRepository.DeleteAsync(photoId);
        return (true, null);
    }

    public async Task<IReadOnlyCollection<HomeProviderGalleryResponse>> GetGalleryAsync(Guid providerId) =>
        (await galleryRepository.GetByProviderIdAsync(providerId))
            .Select(MapPhoto)
            .ToList();

    public async Task<(HomeProviderDocumentResponse? Document, string? ErrorCode)> AddDocumentAsync(
        Guid userId, AddHomeProviderDocumentRequest request)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var document = new HomeProviderDocument
        {
            ProviderId = provider.Id,
            DocumentType = request.DocumentType.Trim(),
            FileUrl = request.FileUrl.Trim()
        };

        var created = await documentRepository.CreateAsync(document);
        return (MapDocument(created), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteDocumentAsync(Guid userId, Guid documentId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (false, "PROVIDER_NOT_FOUND");

        var document = await documentRepository.GetByIdAsync(documentId);
        if (document is null) return (false, "DOCUMENT_NOT_FOUND");
        if (document.ProviderId != provider.Id) return (false, "UNAUTHORIZED");

        await documentRepository.DeleteAsync(documentId);
        return (true, null);
    }

    public async Task<IReadOnlyCollection<HomeProviderDocumentResponse>> GetDocumentsAsync(Guid providerId) =>
        (await documentRepository.GetByProviderIdAsync(providerId))
            .Select(MapDocument)
            .ToList();

    public async Task<(HomeProviderCertificationResponse? Certification, string? ErrorCode)> AddCertificationAsync(
        Guid userId, AddHomeProviderCertificationRequest request)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var certification = new HomeProviderCertification
        {
            ProviderId = provider.Id,
            ServiceTypeId = request.ServiceTypeId,
            Title = request.Title.Trim(),
            IssuingOrganization = request.IssuingOrganization?.Trim(),
            CredentialNumber = request.CredentialNumber?.Trim(),
            IssuedDate = request.IssuedDate,
            ExpiryDate = request.ExpiryDate,
            DocumentUrl = request.DocumentUrl?.Trim()
        };

        var created = await certificationRepository.CreateAsync(certification);
        return (MapCertification(created), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteCertificationAsync(Guid userId, Guid certificationId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (false, "PROVIDER_NOT_FOUND");

        var certification = await certificationRepository.GetByIdAsync(certificationId);
        if (certification is null) return (false, "CERTIFICATION_NOT_FOUND");
        if (certification.ProviderId != provider.Id) return (false, "UNAUTHORIZED");

        await certificationRepository.DeleteAsync(certificationId);
        return (true, null);
    }

    public async Task<IReadOnlyCollection<HomeProviderCertificationResponse>> GetCertificationsAsync(Guid providerId) =>
        (await certificationRepository.GetByProviderIdAsync(providerId))
            .Select(MapCertification)
            .ToList();

    public async Task<(HomeProviderSpecialtyResponse? Specialty, string? ErrorCode)> AddSpecialtyAsync(
        Guid userId, AddHomeProviderSpecialtyRequest request)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var specialty = request.Specialty.Trim();
        if (await specialtyRepository.ExistsAsync(provider.Id, specialty))
            return (null, "SPECIALTY_ALREADY_EXISTS");

        var created = await specialtyRepository.CreateAsync(new HomeProviderSpecialty
        {
            ProviderId = provider.Id,
            Specialty = specialty
        });

        return (MapSpecialty(created), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteSpecialtyAsync(Guid userId, Guid specialtyId)
    {
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null) return (false, "PROVIDER_NOT_FOUND");

        var specialty = await specialtyRepository.GetByIdAsync(specialtyId);
        if (specialty is null) return (false, "SPECIALTY_NOT_FOUND");
        if (specialty.ProviderId != provider.Id) return (false, "UNAUTHORIZED");

        await specialtyRepository.DeleteAsync(specialtyId);
        return (true, null);
    }

    public async Task<IReadOnlyCollection<HomeProviderSpecialtyResponse>> GetSpecialtiesAsync(Guid providerId) =>
        (await specialtyRepository.GetByProviderIdAsync(providerId))
            .Select(MapSpecialty)
            .ToList();

    private static HomeProviderGalleryResponse MapPhoto(HomeProviderGallery g) =>
        new(g.Id, g.ProviderId, g.PhotoUrl, g.CreatedAt);

    private static HomeProviderDocumentResponse MapDocument(HomeProviderDocument d) =>
        new(d.Id, d.ProviderId, d.DocumentType, d.FileUrl, d.CreatedAt);

    private static HomeProviderCertificationResponse MapCertification(HomeProviderCertification c) =>
        new(c.Id, c.ProviderId, c.ServiceTypeId, c.Title, c.IssuingOrganization, c.CredentialNumber,
            c.IssuedDate, c.ExpiryDate, c.DocumentUrl, c.CreatedAt);

    private static HomeProviderSpecialtyResponse MapSpecialty(HomeProviderSpecialty s) =>
        new(s.Id, s.ProviderId, s.Specialty, s.CreatedAt);
}
