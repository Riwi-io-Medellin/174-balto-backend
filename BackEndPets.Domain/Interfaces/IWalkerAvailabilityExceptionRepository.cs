using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IWalkerAvailabilityExceptionRepository
{
    Task<IReadOnlyCollection<WalkerAvailabilityException>> GetByWalkerIdAsync(Guid walkerId);
    /// <summary>Deletes all existing exceptions for the walker and inserts the new set atomically.</summary>
    Task<IReadOnlyCollection<WalkerAvailabilityException>> ReplaceAsync(
        Guid walkerId, IEnumerable<WalkerAvailabilityException> exceptions);
}
