using BackEndPets.Application.DTOs.Bookings;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkBookingService(
    IWalkBookingRepository bookingRepository,
    IWalkerRepository walkerRepository,
    IPetRepository petRepository,
    IAvailabilityEngine availabilityEngine) : IWalkBookingService
{
    // Terminal states — no further transitions allowed from these.
    private static readonly string[] TerminalStatuses =
        ["completed", "rejected", "walker_cancelled", "owner_cancelled"];

    public async Task<(BookingResponse? Result, string? ErrorCode)> CreateAsync(
        Guid clientUserId, CreateBookingRequest request)
    {
        if (request.DurationMinutes is not (30 or 60 or 90))
            return (null, "INVALID_DURATION");

        var pet = await petRepository.GetByIdAsync(request.PetId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != clientUserId) return (null, "PET_NOT_OWNED");

        var walker = await walkerRepository.GetByIdAsync(request.WalkerId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");
        if (walker.VerificationStatus != "approved") return (null, "WALKER_NOT_APPROVED");
        if (!walker.IsAcceptingBookings) return (null, "WALKER_NOT_ACCEPTING_BOOKINGS");

        var slotStartUtc = DateTime.SpecifyKind(request.SlotStart, DateTimeKind.Utc);

        if (slotStartUtc <= DateTime.UtcNow)
            return (null, "SLOT_IN_PAST");

        var slotDate = DateOnly.FromDateTime(slotStartUtc);
        var availableSlots = await availabilityEngine.ComputeSlotsAsync(
            walker.Id, slotDate, request.DurationMinutes);

        if (!availableSlots.Any(s => s.Start == slotStartUtc))
            return (null, "SLOT_NOT_AVAILABLE");

        decimal? totalPrice = walker.HourlyRate.HasValue
            ? Math.Round(walker.HourlyRate.Value * request.DurationMinutes / 60m, 2)
            : null;

        var booking = new WalkBooking
        {
            ClientUserId        = clientUserId,
            WalkerId            = walker.Id,
            PetId               = request.PetId,
            SlotStart           = slotStartUtc,
            DurationMinutes     = request.DurationMinutes,
            SnapshotHourlyRate  = walker.HourlyRate,
            TotalPrice          = totalPrice,
            SpecialInstructions = request.SpecialInstructions?.Trim(),
            Status              = "pending"
        };

        var created = await bookingRepository.CreateAsync(booking);
        return (Map(created), null);
    }

    public async Task<IReadOnlyCollection<BookingResponse>> GetMyBookingsAsync(
        Guid clientUserId, string? status = null) =>
        (await bookingRepository.GetByClientUserIdAsync(clientUserId, status))
            .Select(Map)
            .ToList();

    public async Task<(BookingResponse? Result, string? ErrorCode)> GetByIdAsync(
        Guid currentUserId, Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");

        if (booking.ClientUserId == currentUserId)
            return (Map(booking), null);

        var walker = await walkerRepository.GetByUserIdAsync(currentUserId);
        if (walker is not null && walker.Id == booking.WalkerId)
            return (Map(booking), null);

        return (null, "UNAUTHORIZED");
    }

    public async Task<(IReadOnlyCollection<BookingResponse>? Result, string? ErrorCode)> GetPendingForWalkerAsync(
        Guid walkerUserId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var bookings = await bookingRepository.GetByWalkerIdAsync(walker.Id, status: "pending");
        return (bookings.Select(Map).ToList(), null);
    }

    public async Task<(IReadOnlyCollection<BookingResponse>? Result, string? ErrorCode)> GetWalkerBookingsAsync(
        Guid walkerUserId, string? status = null)
    {
        var walker = await walkerRepository.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var bookings = await bookingRepository.GetByWalkerIdAsync(walker.Id, status);
        return (bookings.Select(Map).ToList(), null);
    }

    public async Task<(BookingResponse? Result, string? ErrorCode)> AcceptAsync(
        Guid walkerUserId, Guid bookingId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.WalkerId != walker.Id) return (null, "UNAUTHORIZED");
        if (booking.Status != "pending") return (null, "BOOKING_NOT_PENDING");

        booking.Status = "accepted";
        var updated = await bookingRepository.UpdateAsync(booking);
        return (Map(updated), null);
    }

    public async Task<(BookingResponse? Result, string? ErrorCode)> RejectAsync(
        Guid walkerUserId, Guid bookingId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.WalkerId != walker.Id) return (null, "UNAUTHORIZED");
        if (booking.Status == "completed") return (null, "CANNOT_REJECT_COMPLETED");
        if (TerminalStatuses.Contains(booking.Status)) return (null, "BOOKING_ALREADY_RESOLVED");

        booking.Status = "rejected";
        var updated = await bookingRepository.UpdateAsync(booking);
        return (Map(updated), null);
    }

    public async Task<(BookingResponse? Result, string? ErrorCode)> CancelByWalkerAsync(
        Guid walkerUserId, Guid bookingId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(walkerUserId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.WalkerId != walker.Id) return (null, "UNAUTHORIZED");
        if (TerminalStatuses.Contains(booking.Status)) return (null, "BOOKING_ALREADY_RESOLVED");
        if (booking.Status == "completed") return (null, "CANNOT_CANCEL_COMPLETED");

        booking.Status = "walker_cancelled";
        var updated = await bookingRepository.UpdateAsync(booking);
        return (Map(updated), null);
    }

    public async Task<(BookingResponse? Result, string? ErrorCode)> CancelByOwnerAsync(
        Guid clientUserId, Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.ClientUserId != clientUserId) return (null, "UNAUTHORIZED");
        if (TerminalStatuses.Contains(booking.Status)) return (null, "BOOKING_ALREADY_RESOLVED");
        if (booking.Status == "completed") return (null, "CANNOT_CANCEL_COMPLETED");

        booking.Status = "owner_cancelled";
        var updated = await bookingRepository.UpdateAsync(booking);
        return (Map(updated), null);
    }

    private static BookingResponse Map(WalkBooking b) => new(
        b.Id, b.ClientUserId, b.WalkerId, b.PetId,
        b.Status, b.SlotStart, b.DurationMinutes,
        b.SnapshotHourlyRate, b.TotalPrice, b.SpecialInstructions,
        b.WalkSessionId, b.CreatedAt, b.UpdatedAt);
}
