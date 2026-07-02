using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Tests.Fakes;

public sealed class FakeWalkerAvailabilityRepository : IWalkerAvailabilityRepository
{
    private readonly List<WalkerAvailability> _slots = [];

    public void Seed(Guid walkerId, int dayOfWeek, TimeOnly start, TimeOnly end) =>
        _slots.Add(new WalkerAvailability
        {
            Id = Guid.NewGuid(),
            WalkerId = walkerId,
            DayOfWeek = dayOfWeek,
            StartTime = start,
            EndTime = end
        });

    public Task<IReadOnlyCollection<WalkerAvailability>> GetByWalkerIdAsync(Guid walkerId) =>
        Task.FromResult<IReadOnlyCollection<WalkerAvailability>>(
            _slots.Where(s => s.WalkerId == walkerId).ToList());

    public Task<IReadOnlyCollection<WalkerAvailability>> ReplaceAsync(
        Guid walkerId, IEnumerable<WalkerAvailability> slots)
    {
        _slots.RemoveAll(s => s.WalkerId == walkerId);
        var toAdd = slots.Select(s => { s.WalkerId = walkerId; s.Id = Guid.NewGuid(); return s; }).ToList();
        _slots.AddRange(toAdd);
        return Task.FromResult<IReadOnlyCollection<WalkerAvailability>>(toAdd);
    }
}
