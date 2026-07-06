using BackEndPets.Application.DTOs.Bookings;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkBookingService(
    IWalkBookingRepository bookingRepository,
    IWalkerRepository walkerRepository,
    IPetRepository petRepository,
    IAvailabilityEngine availabilityEngine,
    INotificationService notificationService,
    IWalkSessionRepository sessionRepository,
    UserManager<ApplicationUser> userManager) : IWalkBookingService
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

        // SlotStart arrives as Colombia local time (no Z suffix) — keep as Unspecified.
        var slotStart = DateTime.SpecifyKind(request.SlotStart, DateTimeKind.Unspecified);

        var colombiaZone  = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        var nowColombia   = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaZone);

        if (slotStart <= nowColombia)
            return (null, "SLOT_IN_PAST");

        var slotDate = DateOnly.FromDateTime(slotStart);
        var availableSlots = await availabilityEngine.ComputeSlotsAsync(
            walker.Id, slotDate, request.DurationMinutes, walker.MaxDogs);

        if (!availableSlots.Any(s => s.Start == slotStart))
            return (null, "SLOT_NOT_AVAILABLE");

        decimal? basePrice = walker.HourlyRate.HasValue
            ? walker.HourlyRate.Value * request.DurationMinutes / 60m
            : null;

        decimal? totalPrice = basePrice.HasValue
            ? Math.Round(request.IsExclusive ? basePrice.Value * 1.5m : basePrice.Value, 2)
            : null;

        var booking = new WalkBooking
        {
            ClientUserId        = clientUserId,
            WalkerId            = walker.Id,
            PetId               = request.PetId,
            SlotStart           = slotStart,
            DurationMinutes     = request.DurationMinutes,
            SnapshotHourlyRate  = walker.HourlyRate,
            TotalPrice          = totalPrice,
            IsExclusive         = request.IsExclusive,
            SpecialInstructions = request.SpecialInstructions?.Trim(),
            Status              = "pending"
        };

        var created = await bookingRepository.CreateAsync(booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: walker.UserId,
            Type: "system",
            Title: "New walk request",
            Body: $"A client has requested a walk for {slotStart:MM/dd/yyyy HH:mm}.",
            EntityId: created.Id,
            EntityType: "walk_booking"));

        return (Map(created), null);
    }

    public async Task<PagedResult<BookingResponse>> GetMyBookingsAsync(
        Guid clientUserId, string? status = null, int page = 1, int pageSize = 20)
    {
        var all = await bookingRepository.GetByClientUserIdAsync(clientUserId, status);
        var totalCount = all.Count;
        var paged = all
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var sessions = await LoadSessionsAsync(paged);
        var walkers = await LoadWalkerProjectionsAsync(paged);
        return new PagedResult<BookingResponse>(paged.Select(b => Map(b, sessions, walkerInfo: walkers.GetValueOrDefault(b.WalkerId))).ToList(), page, pageSize, totalCount);
    }

    public async Task<(BookingResponse? Result, string? ErrorCode)> GetByIdAsync(
        Guid currentUserId, Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");

        WalkSession? session = booking.WalkSessionId.HasValue
            ? await sessionRepository.GetByIdAsync(booking.WalkSessionId.Value)
            : null;

        if (booking.ClientUserId == currentUserId)
            return (Map(booking, session), null);

        var walker = await walkerRepository.GetByUserIdAsync(currentUserId);
        if (walker is not null && walker.Id == booking.WalkerId)
            return (Map(booking, session), null);

        return (null, "UNAUTHORIZED");
    }

    public async Task<PagedResult<BookingResponse>> GetPendingForWalkerAsync(
        Guid walkerUserId, int page = 1, int pageSize = 20)
    {
        var walker = await walkerRepository.GetByUserIdAsync(walkerUserId);
        if (walker is null) return new PagedResult<BookingResponse>([], page, pageSize, 0);

        var all = await bookingRepository.GetByWalkerIdAsync(walker.Id, status: "pending");
        var totalCount = all.Count;
        var paged = all
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var owners = await LoadOwnersAsync(paged);
        return new PagedResult<BookingResponse>(paged.Select(b => Map(b, ownerOf: owners.GetValueOrDefault(b.ClientUserId))).ToList(), page, pageSize, totalCount);
    }

    public async Task<PagedResult<BookingResponse>> GetWalkerBookingsAsync(
        Guid walkerUserId, string? status = null, int page = 1, int pageSize = 20)
    {
        var walker = await walkerRepository.GetByUserIdAsync(walkerUserId);
        if (walker is null) return new PagedResult<BookingResponse>([], page, pageSize, 0);

        var all = await bookingRepository.GetByWalkerIdAsync(walker.Id, status);
        var totalCount = all.Count;
        var paged = all
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var sessions = await LoadSessionsAsync(paged);
        var owners = await LoadOwnersAsync(paged);
        return new PagedResult<BookingResponse>(paged.Select(b => Map(b, sessions, ownerOf: owners.GetValueOrDefault(b.ClientUserId))).ToList(), page, pageSize, totalCount);
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

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "walker_assigned",
            Title: "Booking accepted",
            Body: "The walker has accepted your walking request.",
            EntityId: updated.Id,
            EntityType: "walk_booking"));

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

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "system",
            Title: "Booking rejected",
            Body: "The walker has declined your walking request.",
            EntityId: updated.Id,
            EntityType: "walk_booking"));

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

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "walk_cancelled",
            Title: "Booking cancelled",
            Body: "The walker has cancelled your walking request.",
            EntityId: updated.Id,
            EntityType: "walk_booking"));

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

        var walkerOwner = await walkerRepository.GetByIdAsync(booking.WalkerId);
        if (walkerOwner is not null)
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: walkerOwner.UserId,
                Type: "walk_cancelled",
                Title: "Booking cancelled",
                Body: "The client has cancelled your walking request.",
                EntityId: updated.Id,
                EntityType: "walk_booking"));
        }

        return (Map(updated), null);
    }

    private static BookingResponse Map(WalkBooking b, WalkSession? session = null, ApplicationUser? ownerOf = null, WalkerUserProjection? walkerInfo = null) => new(
        b.Id, b.ClientUserId, b.WalkerId, b.PetId,
        b.Status, b.SlotStart, b.DurationMinutes,
        b.SnapshotHourlyRate, b.TotalPrice, b.IsExclusive, b.SpecialInstructions,
        b.WalkSessionId, b.CreatedAt, b.UpdatedAt,
        session?.TotalDistanceMeters,
        session?.TotalDurationSeconds,
        ownerOf?.Latitude,
        ownerOf?.Longitude,
        ownerOf?.Address ?? ownerOf?.Location,
        walkerInfo is not null ? $"{walkerInfo.FirstName} {walkerInfo.LastName}" : null,
        walkerInfo?.PhotoUrl);

    private static BookingResponse Map(WalkBooking b, IReadOnlyDictionary<Guid, WalkSession> sessions, ApplicationUser? ownerOf = null, WalkerUserProjection? walkerInfo = null) =>
        Map(b, b.WalkSessionId.HasValue ? sessions.GetValueOrDefault(b.WalkSessionId.Value) : null, ownerOf, walkerInfo);

    private async Task<IReadOnlyDictionary<Guid, WalkSession>> LoadSessionsAsync(IEnumerable<WalkBooking> bookings)
    {
        var ids = bookings
            .Where(b => b.WalkSessionId.HasValue)
            .Select(b => b.WalkSessionId!.Value)
            .Distinct();
        return await sessionRepository.GetByIdsAsync(ids);
    }

    private async Task<IReadOnlyDictionary<Guid, ApplicationUser>> LoadOwnersAsync(IEnumerable<WalkBooking> bookings)
    {
        var result = new Dictionary<Guid, ApplicationUser>();
        foreach (var userId in bookings.Select(b => b.ClientUserId).Distinct())
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null) result[userId] = user;
        }
        return result;
    }

    private async Task<IReadOnlyDictionary<Guid, WalkerUserProjection>> LoadWalkerProjectionsAsync(IEnumerable<WalkBooking> bookings)
    {
        var result = new Dictionary<Guid, WalkerUserProjection>();
        foreach (var walkerId in bookings.Select(b => b.WalkerId).Distinct())
        {
            var projection = await walkerRepository.GetByIdWithUserAsync(walkerId);
            if (projection is not null) result[walkerId] = projection;
        }
        return result;
    }
}
