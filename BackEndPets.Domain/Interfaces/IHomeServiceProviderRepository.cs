using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeServiceProviderRepository
{
    Task<HomeServiceProvider?> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsForUserAsync(Guid userId);
    Task<HomeServiceProvider> CreateAsync(HomeServiceProvider provider);
    Task<IReadOnlyCollection<HomeServiceProvider>> GetAllAsync(bool? isAcceptingBookings = null, string? baseLocation = null);
    Task<HomeServiceProvider?> GetByIdAsync(Guid id);
    Task<HomeServiceProvider> UpdateAsync(HomeServiceProvider provider);
    Task<IReadOnlyCollection<HomeServiceProviderUserProjection>> GetAllWithUserAsync(
        bool? isAcceptingBookings = null, string? baseLocation = null);
    Task<IReadOnlyCollection<HomeServiceProviderUserProjection>> GetApprovedAcceptingWithUserAsync();
    Task<HomeServiceProviderUserProjection?> GetByIdWithUserAsync(Guid id);
}
