using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Tests.Fakes;

public sealed class FakeWalkerAvailabilityExceptionRepository : IWalkerAvailabilityExceptionRepository
{
    private readonly List<WalkerAvailabilityException> _exceptions = [];

    public void SeedUnavailable(Guid walkerId, DateOnly date) =>
        _exceptions.Add(new WalkerAvailabilityException
        {
            Id = Guid.NewGuid(),
            WalkerId = walkerId,
            Date = date,
            IsUnavailable = true
        });

    public void SeedAlternateHours(Guid walkerId, DateOnly date, TimeOnly start, TimeOnly end) =>
        _exceptions.Add(new WalkerAvailabilityException
        {
            Id = Guid.NewGuid(),
            WalkerId = walkerId,
            Date = date,
            IsUnavailable = false,
            StartTime = start,
            EndTime = end
        });

    public Task<IReadOnlyCollection<WalkerAvailabilityException>> GetByWalkerIdAsync(Guid walkerId) =>
        Task.FromResult<IReadOnlyCollection<WalkerAvailabilityException>>(
            _exceptions.Where(e => e.WalkerId == walkerId).ToList());

    public Task<IReadOnlyCollection<WalkerAvailabilityException>> ReplaceAsync(
        Guid walkerId, IEnumerable<WalkerAvailabilityException> exceptions)
    {
        _exceptions.RemoveAll(e => e.WalkerId == walkerId);
        var toAdd = exceptions.Select(e => { e.WalkerId = walkerId; e.Id = Guid.NewGuid(); return e; }).ToList();
        _exceptions.AddRange(toAdd);
        return Task.FromResult<IReadOnlyCollection<WalkerAvailabilityException>>(toAdd);
    }
}
