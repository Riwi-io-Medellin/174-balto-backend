using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Tests.Fakes;

public sealed class FakeWalkBookingRepository : IWalkBookingRepository
{
    private readonly List<WalkBooking> _bookings = [];

    public void Seed(Guid walkerId, DateTime slotStart, int durationMinutes, string status = "accepted")
    {
        _bookings.Add(new WalkBooking
        {
            Id = Guid.NewGuid(),
            WalkerId = walkerId,
            SlotStart = slotStart,
            DurationMinutes = durationMinutes,
            Status = status
        });
    }

    public Task<WalkBooking> CreateAsync(WalkBooking booking)
    {
        booking.Id = Guid.NewGuid();
        _bookings.Add(booking);
        return Task.FromResult(booking);
    }

    public Task<WalkBooking?> GetByIdAsync(Guid id) =>
        Task.FromResult(_bookings.FirstOrDefault(b => b.Id == id));

    public Task<IReadOnlyCollection<WalkBooking>> GetByClientUserIdAsync(Guid clientUserId, string? status = null) =>
        Task.FromResult<IReadOnlyCollection<WalkBooking>>(
            _bookings.Where(b => b.ClientUserId == clientUserId && (status == null || b.Status == status)).ToList());

    public Task<IReadOnlyCollection<WalkBooking>> GetByWalkerIdAsync(Guid walkerId, string? status = null) =>
        Task.FromResult<IReadOnlyCollection<WalkBooking>>(
            _bookings.Where(b => b.WalkerId == walkerId && (status == null || b.Status == status)).ToList());

    public Task<WalkBooking> UpdateAsync(WalkBooking booking)
    {
        var index = _bookings.FindIndex(b => b.Id == booking.Id);
        if (index >= 0) _bookings[index] = booking;
        return Task.FromResult(booking);
    }

    public Task<(WalkBooking Booking, WalkSession Session)> AcceptWithSessionAsync(
        WalkBooking booking, WalkSession session) =>
        throw new NotImplementedException("Not needed for availability engine tests.");
}
