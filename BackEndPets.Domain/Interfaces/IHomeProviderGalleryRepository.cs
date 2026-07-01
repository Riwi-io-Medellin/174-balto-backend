using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderGalleryRepository
{
    Task<HomeProviderGallery> CreateAsync(HomeProviderGallery photo);
    Task<IReadOnlyCollection<HomeProviderGallery>> GetByProviderIdAsync(Guid providerId);
    Task<HomeProviderGallery?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
