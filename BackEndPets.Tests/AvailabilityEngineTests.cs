using BackEndPets.Infrastructure.Services;
using BackEndPets.Tests.Fakes;
using Xunit;

namespace BackEndPets.Tests;

public sealed class AvailabilityEngineTests
{
    // Wednesday, chosen arbitrarily and kept fixed across tests.
    private static readonly DateOnly TestDate = new(2026, 7, 1);

    private static (AvailabilityEngine Engine, FakeWalkerAvailabilityRepository Availability,
        FakeWalkerAvailabilityExceptionRepository Exceptions, FakeWalkBookingRepository Bookings) Build()
    {
        var availability = new FakeWalkerAvailabilityRepository();
        var exceptions = new FakeWalkerAvailabilityExceptionRepository();
        var bookings = new FakeWalkBookingRepository();
        var engine = new AvailabilityEngine(availability, exceptions, bookings);
        return (engine, availability, exceptions, bookings);
    }

    private static DateTime At(int hour, int minute) =>
        new(TestDate.Year, TestDate.Month, TestDate.Day, hour, minute, 0);

    [Fact]
    public async Task ComputeSlots_NoBookings_GeneratesEvery30MinutesWithinWindow()
    {
        var (engine, availability, _, _) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(16, 0));

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        // 2:00–4:00 window, 60-min occupied block per slot -> last valid start is 3:00 (occupies 3:00-4:00).
        Assert.Equal(
            new[] { At(14, 0), At(14, 30), At(15, 0) },
            slots.Select(s => s.Start));
    }

    [Fact]
    public async Task ComputeSlots_BookingAt2PM_Blocks230_AllowsNextValidAt3PM()
    {
        // Matches the spec example exactly: booking at 2:00 PM occupies 2:00-3:00 PM.
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(17, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 30);

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);
        var starts = slots.Select(s => s.Start).ToList();

        Assert.DoesNotContain(At(14, 0), starts);
        Assert.DoesNotContain(At(14, 30), starts);
        Assert.Contains(At(15, 0), starts);
    }

    [Theory]
    [InlineData(14, 0)]   // exact same start
    [InlineData(14, 15)]  // starts mid-occupied-interval
    [InlineData(14, 30)]  // starts right after the walk portion but still within pickup/travel buffer
    public async Task ComputeSlots_CandidateOverlappingOccupiedInterval_IsExcluded(int hour, int minute)
    {
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(17, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 30);

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        Assert.DoesNotContain(At(hour, minute), slots.Select(s => s.Start));
    }

    [Fact]
    public async Task ComputeSlots_CandidateStartingExactlyAtOccupiedEnd_IsAllowed()
    {
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(17, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 30); // occupies 14:00-15:00

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        Assert.Contains(At(15, 0), slots.Select(s => s.Start));
    }

    [Fact]
    public async Task ComputeSlots_LongerWalkDuration_ReservesProportionallyLargerBlock()
    {
        // A 60-minute walk occupies 15 + 60 + 15 = 90 minutes.
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(18, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 60); // occupies 14:00-15:30

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);
        var starts = slots.Select(s => s.Start).ToList();

        Assert.DoesNotContain(At(15, 0), starts); // still inside 14:00-15:30 occupied block
        Assert.Contains(At(15, 30), starts);
    }

    [Fact]
    public async Task ComputeSlots_SlotWhoseOwnOccupiedBlockWouldExceedWindow_IsExcluded()
    {
        // Window closes at 16:00. A 30-min walk starting 15:30 would occupy 15:30-16:30, past closing.
        var (engine, availability, _, _) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(16, 0));

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        Assert.DoesNotContain(At(15, 30), slots.Select(s => s.Start));
        Assert.Contains(At(15, 0), slots.Select(s => s.Start)); // occupies 15:00-16:00, fits exactly
    }

    [Fact]
    public async Task ComputeSlots_PendingBookingAlsoBlocks_NotJustAccepted()
    {
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(16, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 30, status: "pending");

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        Assert.DoesNotContain(At(14, 30), slots.Select(s => s.Start));
    }

    [Theory]
    [InlineData("rejected")]
    [InlineData("walker_cancelled")]
    [InlineData("owner_cancelled")]
    [InlineData("completed")]
    public async Task ComputeSlots_TerminalBookingStatuses_DoNotBlock(string status)
    {
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(16, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 30, status: status);

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        Assert.Contains(At(14, 0), slots.Select(s => s.Start));
    }

    [Fact]
    public async Task ComputeSlots_MaxDogsCapacityAbove1_AllowsOverlapUpToCapacity()
    {
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(16, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 30); // 1 of 2 capacity used for 14:00-15:00

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 2);

        Assert.Contains(At(14, 0), slots.Select(s => s.Start)); // second dog can still be booked
    }

    [Fact]
    public async Task ComputeSlots_NullMaxDogs_DefaultsToCapacityOfOneAndStillBlocksOverlaps()
    {
        // Regression guard: previously a null maxDogs skipped overlap filtering entirely,
        // which allowed double-booking a walker who hadn't set a MaxDogs value.
        var (engine, availability, _, bookings) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(16, 0));
        bookings.Seed(walkerId, At(14, 0), durationMinutes: 30);

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: null);

        Assert.DoesNotContain(At(14, 0), slots.Select(s => s.Start));
        Assert.DoesNotContain(At(14, 30), slots.Select(s => s.Start));
        Assert.Contains(At(15, 0), slots.Select(s => s.Start));
    }

    [Fact]
    public async Task ComputeSlots_DateException_MarkedUnavailable_ReturnsNoSlots()
    {
        var (engine, availability, exceptions, _) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(14, 0), new TimeOnly(16, 0));
        exceptions.SeedUnavailable(walkerId, TestDate);

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        Assert.Empty(slots);
    }

    [Fact]
    public async Task ComputeSlots_DateException_AlternateHours_OverridesWeeklySchedule()
    {
        var (engine, availability, exceptions, _) = Build();
        var walkerId = Guid.NewGuid();
        availability.Seed(walkerId, (int)TestDate.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(11, 0));
        exceptions.SeedAlternateHours(walkerId, TestDate, new TimeOnly(14, 0), new TimeOnly(15, 0));

        var slots = await engine.ComputeSlotsAsync(walkerId, TestDate, durationMinutes: 30, maxDogs: 1);

        Assert.Single(slots);
        Assert.Equal(At(14, 0), slots.Single().Start);
    }
}
