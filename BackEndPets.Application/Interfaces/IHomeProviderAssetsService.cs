using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeProviderAssetsService
{
    Task<(HomeProviderGalleryResponse? Photo, string? ErrorCode)> AddPhotoAsync(
        Guid userId, AddHomeProviderPhotoRequest request);
    Task<(bool Success, string? ErrorCode)> DeletePhotoAsync(Guid userId, Guid photoId);
    Task<IReadOnlyCollection<HomeProviderGalleryResponse>> GetGalleryAsync(Guid providerId);

    Task<(HomeProviderDocumentResponse? Document, string? ErrorCode)> AddDocumentAsync(
        Guid userId, AddHomeProviderDocumentRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteDocumentAsync(Guid userId, Guid documentId);
    Task<IReadOnlyCollection<HomeProviderDocumentResponse>> GetDocumentsAsync(Guid providerId);

    Task<(HomeProviderCertificationResponse? Certification, string? ErrorCode)> AddCertificationAsync(
        Guid userId, AddHomeProviderCertificationRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteCertificationAsync(Guid userId, Guid certificationId);
    Task<IReadOnlyCollection<HomeProviderCertificationResponse>> GetCertificationsAsync(Guid providerId);

    Task<(HomeProviderSpecialtyResponse? Specialty, string? ErrorCode)> AddSpecialtyAsync(
        Guid userId, AddHomeProviderSpecialtyRequest request);
    Task<(bool Success, string? ErrorCode)> DeleteSpecialtyAsync(Guid userId, Guid specialtyId);
    Task<IReadOnlyCollection<HomeProviderSpecialtyResponse>> GetSpecialtiesAsync(Guid providerId);
}
