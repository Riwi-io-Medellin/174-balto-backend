using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkerGalleryRepository
{
    Task<WalkerGallery> CreateAsync(WalkerGallery photo);
    Task<IReadOnlyCollection<WalkerGallery>> GetByWalkerIdAsync(Guid walkerId);
    Task<WalkerGallery?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}