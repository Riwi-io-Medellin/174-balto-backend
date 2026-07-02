using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderServiceAreaRepository
{
    Task<IReadOnlyCollection<HomeProviderServiceArea>> GetByProviderIdAsync(Guid providerId);
    /// <summary>Deletes all existing areas for the provider and inserts the new set atomically.</summary>
    Task<IReadOnlyCollection<HomeProviderServiceArea>> ReplaceAsync(
        Guid providerId, IEnumerable<HomeProviderServiceArea> areas);
    Task<IReadOnlyCollection<HomeProviderServiceArea>> GetAllAsync();
}
