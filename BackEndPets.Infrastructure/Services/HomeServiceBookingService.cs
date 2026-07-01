using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceBookingService(
    IHomeServiceBookingRepository bookingRepository,
    IHomeServiceProviderRepository providerRepository,
    IPetRepository petRepository,
    IHomeProviderServiceRepository providerServiceRepository,
    IHomeServiceAvailabilityEngine availabilityEngine,
    INotificationService notificationService) : IHomeServiceBookingService
{
    // Terminal states — no further transitions allowed from these.
    private static readonly string[] TerminalStatuses =
        ["completed", "rejected", "provider_cancelled", "client_cancelled"];

    public async Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> CreateAsync(
        Guid clientUserId, CreateHomeServiceBookingRequest request)
    {
        if (request.DurationMinutes is not (30 or 60 or 90))
            return (null, "INVALID_DURATION");

        var pet = await petRepository.GetByIdAsync(request.PetId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != clientUserId) return (null, "PET_NOT_OWNED");

        var provider = await providerRepository.GetByIdAsync(request.ProviderId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");
        if (provider.VerificationStatus != "approved") return (null, "PROVIDER_NOT_APPROVED");
        if (!provider.IsAcceptingBookings) return (null, "PROVIDER_NOT_ACCEPTING_BOOKINGS");

        var offeredService = (await providerServiceRepository.GetByProviderIdAsync(provider.Id))
            .FirstOrDefault(s => s.ServiceTypeId == request.ServiceTypeId && s.IsActive);
        if (offeredService is null) return (null, "SERVICE_NOT_OFFERED");

        // SlotStart arrives as Colombia local time (no Z suffix) — keep as Unspecified.
        var slotStart = DateTime.SpecifyKind(request.SlotStart, DateTimeKind.Unspecified);

        var colombiaZone  = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        var nowColombia   = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaZone);

        if (slotStart <= nowColombia)
            return (null, "SLOT_IN_PAST");

        var slotDate = DateOnly.FromDateTime(slotStart);
        var availableSlots = await availabilityEngine.ComputeSlotsAsync(
            provider.Id, slotDate, request.DurationMinutes, provider.MaxConcurrentBookings);

        if (!availableSlots.Any(s => s.Start == slotStart))
            return (null, "SLOT_NOT_AVAILABLE");

        decimal? totalPrice = offeredService.Price.HasValue
            ? offeredService.PriceUnit == "hourly"
                ? Math.Round(offeredService.Price.Value * request.DurationMinutes / 60m, 2)
                : offeredService.Price.Value
            : null;

        var booking = new HomeServiceBooking
        {
            ClientUserId         = clientUserId,
            ProviderId           = provider.Id,
            ServiceTypeId        = request.ServiceTypeId,
            PetId                = request.PetId,
            SlotStart            = slotStart,
            DurationMinutes      = request.DurationMinutes,
            SnapshotPrice        = offeredService.Price,
            TotalPrice           = totalPrice,
            ServiceAddress       = request.ServiceAddress?.Trim(),
            ServiceLatitude      = request.ServiceLatitude,
            ServiceLongitude     = request.ServiceLongitude,
            SpecialInstructions  = request.SpecialInstructions?.Trim(),
            Status               = "pending"
        };

        var created = await bookingRepository.CreateAsync(booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: provider.UserId,
            Type: "system",
            Title: "Nueva solicitud de servicio",
            Body: $"Un cliente ha solicitado un servicio para el {slotStart:dd/MM/yyyy HH:mm}.",
            EntityId: created.Id,
            EntityType: "home_service_booking"));

        return (Map(created), null);
    }

    public async Task<PagedResult<HomeServiceBookingResponse>> GetMyBookingsAsync(
        Guid clientUserId, string? status = null, int page = 1, int pageSize = 20)
    {
        var all = await bookingRepository.GetByClientUserIdAsync(clientUserId, status);
        var totalCount = all.Count;
        var paged = all
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var providers = await LoadProviderProjectionsAsync(paged);
        return new PagedResult<HomeServiceBookingResponse>(
            paged.Select(b => Map(b, providers.GetValueOrDefault(b.ProviderId))).ToList(), page, pageSize, totalCount);
    }

    public async Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> GetByIdAsync(
        Guid currentUserId, Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");

        if (booking.ClientUserId == currentUserId)
            return (Map(booking), null);

        var provider = await providerRepository.GetByUserIdAsync(currentUserId);
        if (provider is not null && provider.Id == booking.ProviderId)
            return (Map(booking), null);

        return (null, "UNAUTHORIZED");
    }

    public async Task<PagedResult<HomeServiceBookingResponse>> GetPendingForProviderAsync(
        Guid providerUserId, int page = 1, int pageSize = 20)
    {
        var provider = await providerRepository.GetByUserIdAsync(providerUserId);
        if (provider is null) return new PagedResult<HomeServiceBookingResponse>([], page, pageSize, 0);

        var all = await bookingRepository.GetByProviderIdAsync(provider.Id, status: "pending");
        var totalCount = all.Count;
        var paged = all
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PagedResult<HomeServiceBookingResponse>(paged.Select(b => Map(b)).ToList(), page, pageSize, totalCount);
    }

    public async Task<PagedResult<HomeServiceBookingResponse>> GetProviderBookingsAsync(
        Guid providerUserId, string? status = null, int page = 1, int pageSize = 20)
    {
        var provider = await providerRepository.GetByUserIdAsync(providerUserId);
        if (provider is null) return new PagedResult<HomeServiceBookingResponse>([], page, pageSize, 0);

        var all = await bookingRepository.GetByProviderIdAsync(provider.Id, status);
        var totalCount = all.Count;
        var paged = all
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PagedResult<HomeServiceBookingResponse>(paged.Select(b => Map(b)).ToList(), page, pageSize, totalCount);
    }

    public async Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> AcceptAsync(
        Guid providerUserId, Guid bookingId)
    {
        var provider = await providerRepository.GetByUserIdAsync(providerUserId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.ProviderId != provider.Id) return (null, "UNAUTHORIZED");
        if (booking.Status != "pending") return (null, "BOOKING_NOT_PENDING");

        booking.Status = "accepted";
        booking.AcceptedAt = DateTime.UtcNow;
        var updated = await bookingRepository.UpdateAsync(booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "system",
            Title: "Servicio aceptado",
            Body: "El proveedor ha aceptado tu solicitud de servicio.",
            EntityId: updated.Id,
            EntityType: "home_service_booking"));

        return (Map(updated), null);
    }

    public async Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> RejectAsync(
        Guid providerUserId, Guid bookingId)
    {
        var provider = await providerRepository.GetByUserIdAsync(providerUserId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.ProviderId != provider.Id) return (null, "UNAUTHORIZED");
        if (booking.Status == "completed") return (null, "CANNOT_REJECT_COMPLETED");
        if (TerminalStatuses.Contains(booking.Status)) return (null, "BOOKING_ALREADY_RESOLVED");

        booking.Status = "rejected";
        booking.RejectedAt = DateTime.UtcNow;
        var updated = await bookingRepository.UpdateAsync(booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "system",
            Title: "Servicio rechazado",
            Body: "El proveedor ha rechazado tu solicitud de servicio.",
            EntityId: updated.Id,
            EntityType: "home_service_booking"));

        return (Map(updated), null);
    }

    public async Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> CancelByProviderAsync(
        Guid providerUserId, Guid bookingId)
    {
        var provider = await providerRepository.GetByUserIdAsync(providerUserId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.ProviderId != provider.Id) return (null, "UNAUTHORIZED");
        if (TerminalStatuses.Contains(booking.Status)) return (null, "BOOKING_ALREADY_RESOLVED");
        if (booking.Status == "completed") return (null, "CANNOT_CANCEL_COMPLETED");

        booking.Status = "provider_cancelled";
        booking.CancelledAt = DateTime.UtcNow;
        var updated = await bookingRepository.UpdateAsync(booking);

        await notificationService.CreateAsync(new CreateNotificationRequest(
            UserId: booking.ClientUserId,
            Type: "system",
            Title: "Servicio cancelado",
            Body: "El proveedor ha cancelado el servicio.",
            EntityId: updated.Id,
            EntityType: "home_service_booking"));

        return (Map(updated), null);
    }

    public async Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> CancelByClientAsync(
        Guid clientUserId, Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) return (null, "BOOKING_NOT_FOUND");
        if (booking.ClientUserId != clientUserId) return (null, "UNAUTHORIZED");
        if (TerminalStatuses.Contains(booking.Status)) return (null, "BOOKING_ALREADY_RESOLVED");
        if (booking.Status == "completed") return (null, "CANNOT_CANCEL_COMPLETED");

        booking.Status = "client_cancelled";
        booking.CancelledAt = DateTime.UtcNow;
        var updated = await bookingRepository.UpdateAsync(booking);

        var providerOwner = await providerRepository.GetByIdAsync(booking.ProviderId);
        if (providerOwner is not null)
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: providerOwner.UserId,
                Type: "system",
                Title: "Servicio cancelado",
                Body: "El cliente ha cancelado el servicio.",
                EntityId: updated.Id,
                EntityType: "home_service_booking"));
        }

        return (Map(updated), null);
    }

    private static HomeServiceBookingResponse Map(HomeServiceBooking b, HomeServiceProviderUserProjection? providerInfo = null) => new(
        b.Id, b.ClientUserId, b.ProviderId, b.ServiceTypeId, b.PetId,
        b.Status, b.SlotStart, b.DurationMinutes,
        b.SnapshotPrice, b.TotalPrice,
        b.ServiceAddress, b.ServiceLatitude, b.ServiceLongitude, b.SpecialInstructions,
        b.HomeServiceSessionId, b.CreatedAt, b.UpdatedAt,
        providerInfo is not null ? $"{providerInfo.FirstName} {providerInfo.LastName}" : null,
        providerInfo?.PhotoUrl);

    private async Task<IReadOnlyDictionary<Guid, HomeServiceProviderUserProjection>> LoadProviderProjectionsAsync(
        IEnumerable<HomeServiceBooking> bookings)
    {
        var result = new Dictionary<Guid, HomeServiceProviderUserProjection>();
        foreach (var providerId in bookings.Select(b => b.ProviderId).Distinct())
        {
            var projection = await providerRepository.GetByIdWithUserAsync(providerId);
            if (projection is not null) result[providerId] = projection;
        }
        return result;
    }
}
