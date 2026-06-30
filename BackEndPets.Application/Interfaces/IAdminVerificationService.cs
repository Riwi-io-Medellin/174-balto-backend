using BackEndPets.Application.DTOs.Admin;

namespace BackEndPets.Application.Interfaces;

public interface IAdminVerificationService
{
    Task<IReadOnlyCollection<AdminBusinessVerificationResponse>> GetBusinessesAsync(string? status = null);
    Task<AdminBusinessVerificationResponse?> UpdateBusinessStatusAsync(Guid businessId, string verificationStatus);
    Task<IReadOnlyCollection<AdminWalkerVerificationResponse>> GetWalkersAsync(string? status = null);
    Task<AdminWalkerVerificationResponse?> UpdateWalkerStatusAsync(Guid walkerId, string verificationStatus);
}
