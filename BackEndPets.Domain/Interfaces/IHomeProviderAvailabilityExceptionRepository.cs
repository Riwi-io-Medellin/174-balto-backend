using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IHomeProviderAvailabilityExceptionRepository
{
    Task<IReadOnlyCollection<HomeProviderAvailabilityException>> GetByProviderIdAsync(Guid providerId);
    /// <summary>Deletes all existing exceptions for the provider and inserts the new set atomically.</summary>
    Task<IReadOnlyCollection<HomeProviderAvailabilityException>> ReplaceAsync(
        Guid providerId, IEnumerable<HomeProviderAvailabilityException> exceptions);
}
