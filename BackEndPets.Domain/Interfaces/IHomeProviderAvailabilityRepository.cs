using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderAvailabilityRepository
{
    Task<IReadOnlyCollection<HomeProviderAvailability>> GetByProviderIdAsync(Guid providerId);
    /// <summary>Deletes all existing slots for the provider and inserts the new set atomically.</summary>
    Task<IReadOnlyCollection<HomeProviderAvailability>> ReplaceAsync(
        Guid providerId, IEnumerable<HomeProviderAvailability> slots);
}
