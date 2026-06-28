using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkerAvailabilityRepository
{
    Task<IReadOnlyCollection<WalkerAvailability>> GetByWalkerIdAsync(Guid walkerId);
    /// <summary>Deletes all existing slots for the walker and inserts the new set atomically.</summary>
    Task<IReadOnlyCollection<WalkerAvailability>> ReplaceAsync(
        Guid walkerId, IEnumerable<WalkerAvailability> slots);
}
