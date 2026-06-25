using BackEndPets.Application.DTOs.Profiles;

namespace BackEndPets.Application.Interfaces;

public interface IProfileService
{
    Task<MeResponse?> GetMeAsync(Guid userId);
    Task<(WalkerResponse? Walker, string? ErrorCode)> BecomeWalkerAsync(Guid userId);
    Task<(BusinessResponse? Business, string? ErrorCode)> CreateBusinessAsync(Guid userId, CreateBusinessRequest request);
    Task<IReadOnlyCollection<WalkerResponse>> GetWalkersAsync();
    Task<WalkerResponse?> GetWalkerByIdAsync(Guid id);
    Task<IReadOnlyCollection<BusinessResponse>> GetBusinessesAsync();
    Task<BusinessResponse?> GetBusinessByIdAsync(Guid id);
}
