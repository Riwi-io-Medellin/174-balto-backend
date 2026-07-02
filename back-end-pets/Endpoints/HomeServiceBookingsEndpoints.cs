using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class HomeServiceBookingsEndpoints
{
    public static IEndpointRouteBuilder MapHomeServiceBookingsEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Client endpoints ──────────────────────────────────────────────────

        var bookings = app.MapGroup("/api/home-service-bookings")
            .WithTags("HomeServiceBookings")
            .RequireAuthorization();

        bookings.MapPost("/", async (
            CreateHomeServiceBookingRequest request,
            HttpContext ctx,
            IHomeServiceBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.CreateAsync(userId, request);
            return errorCode switch
            {
                "INVALID_DURATION"                => Results.BadRequest(new ApiErrorResponse("durationMinutes must be 30, 60, or 90.", "INVALID_DURATION")),
                "PET_NOT_FOUND"                    => Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                "PET_NOT_OWNED"                    => Results.Json(new ApiErrorResponse("Pet does not belong to you.", "PET_NOT_OWNED"), statusCode: StatusCodes.Status403Forbidden),
                "PROVIDER_NOT_FOUND"               => Results.NotFound(new ApiErrorResponse("Home service provider not found.", "PROVIDER_NOT_FOUND")),
                "PROVIDER_NOT_APPROVED"            => Results.Conflict(new ApiErrorResponse("Provider is not approved.", "PROVIDER_NOT_APPROVED")),
                "PROVIDER_NOT_ACCEPTING_BOOKINGS"  => Results.Conflict(new ApiErrorResponse("Provider is not accepting bookings.", "PROVIDER_NOT_ACCEPTING_BOOKINGS")),
                "SERVICE_NOT_OFFERED"              => Results.Conflict(new ApiErrorResponse("Provider does not offer this service type.", "SERVICE_NOT_OFFERED")),
                "SLOT_IN_PAST"                     => Results.BadRequest(new ApiErrorResponse("Slot start must be in the future.", "SLOT_IN_PAST")),
                "SLOT_NOT_AVAILABLE"               => Results.Conflict(new ApiErrorResponse("Requested slot is not available for this provider.", "SLOT_NOT_AVAILABLE")),
                _ when result is not null          => Results.Created($"/api/home-service-bookings/{result.Id}", result),
                _                                  => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CreateHomeServiceBooking")
        .WithSummary("Create a new home service booking")
        .Produces<HomeServiceBookingResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapGet("/me", async (
            HttpContext ctx,
            IHomeServiceBookingService service,
            string? status = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await service.GetMyBookingsAsync(userId, status, page, pageSize);
            return Results.Ok(result);
        })
        .WithName("GetMyHomeServiceBookings")
        .WithSummary("Get the authenticated user's home service bookings")
        .Produces<PagedResult<HomeServiceBookingResponse>>(StatusCodes.Status200OK);

        bookings.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext ctx,
            IHomeServiceBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetByIdAsync(userId, id);
            return errorCode switch
            {
                "BOOKING_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"      => Results.Json(new ApiErrorResponse("Access denied.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetHomeServiceBookingById")
        .WithSummary("Get booking details (client or assigned provider only)")
        .Produces<HomeServiceBookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        bookings.MapPost("/{id:guid}/accept", async (
            Guid id,
            HttpContext ctx,
            IHomeServiceBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.AcceptAsync(userId, id);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "BOOKING_NOT_FOUND"  => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"       => Results.Json(new ApiErrorResponse("This booking is not assigned to you.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_NOT_PENDING" => Results.Conflict(new ApiErrorResponse("Booking is not in pending status.", "BOOKING_NOT_PENDING")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AcceptHomeServiceBooking")
        .WithSummary("Accept a pending booking (provider only)")
        .Produces<HomeServiceBookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapPost("/{id:guid}/reject", async (
            Guid id,
            HttpContext ctx,
            IHomeServiceBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.RejectAsync(userId, id);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND"      => Results.NotFound(new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "BOOKING_NOT_FOUND"       => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"            => Results.Json(new ApiErrorResponse("This booking is not assigned to you.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "CANNOT_REJECT_COMPLETED" => Results.Conflict(new ApiErrorResponse("Cannot reject a completed booking.", "CANNOT_REJECT_COMPLETED")),
                "BOOKING_ALREADY_RESOLVED"=> Results.Conflict(new ApiErrorResponse("Booking is already in a terminal state.", "BOOKING_ALREADY_RESOLVED")),
                _ when result is not null => Results.Ok(result),
                _                         => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("RejectHomeServiceBooking")
        .WithSummary("Reject a booking (provider only)")
        .Produces<HomeServiceBookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapPost("/{id:guid}/cancel", async (
            Guid id,
            HttpContext ctx,
            IHomeServiceBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.CancelByProviderAsync(userId, id);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND"      => Results.NotFound(new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "BOOKING_NOT_FOUND"       => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"            => Results.Json(new ApiErrorResponse("This booking is not assigned to you.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_ALREADY_RESOLVED"=> Results.Conflict(new ApiErrorResponse("Booking is already in a terminal state.", "BOOKING_ALREADY_RESOLVED")),
                "CANNOT_CANCEL_COMPLETED" => Results.Conflict(new ApiErrorResponse("Cannot cancel a completed booking.", "CANNOT_CANCEL_COMPLETED")),
                _ when result is not null => Results.Ok(result),
                _                         => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CancelHomeServiceBookingByProvider")
        .WithSummary("Cancel a booking as the assigned provider (sets status to provider_cancelled)")
        .Produces<HomeServiceBookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapPost("/{id:guid}/client-cancel", async (
            Guid id,
            HttpContext ctx,
            IHomeServiceBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.CancelByClientAsync(userId, id);
            return errorCode switch
            {
                "BOOKING_NOT_FOUND"       => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"            => Results.Json(new ApiErrorResponse("You do not own this booking.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_ALREADY_RESOLVED"=> Results.Conflict(new ApiErrorResponse("Booking is already in a terminal state.", "BOOKING_ALREADY_RESOLVED")),
                "CANNOT_CANCEL_COMPLETED" => Results.Conflict(new ApiErrorResponse("Cannot cancel a completed booking.", "CANNOT_CANCEL_COMPLETED")),
                _ when result is not null => Results.Ok(result),
                _                         => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CancelHomeServiceBookingByClient")
        .WithSummary("Cancel a booking as the booking client (sets status to client_cancelled)")
        .Produces<HomeServiceBookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // ── Provider-facing endpoints ─────────────────────────────────────────

        var providers = app.MapGroup("/api/home-services/providers")
            .WithTags("HomeServices")
            .RequireAuthorization();

        providers.MapGet("/me/bookings", async (
            HttpContext ctx,
            IHomeServiceBookingService service,
            string? status = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await service.GetProviderBookingsAsync(userId, status, page, pageSize);
            if (result.TotalCount == 0 && !string.IsNullOrEmpty(status))
            {
                return Results.NotFound(new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND"));
            }
            return Results.Ok(result);
        })
        .WithName("GetHomeServiceProviderBookings")
        .WithSummary("Get all bookings for the authenticated provider (optional status filter)")
        .Produces<PagedResult<HomeServiceBookingResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        providers.MapGet("/me/pending-bookings", async (
            HttpContext ctx,
            IHomeServiceBookingService service,
            int page = 1,
            int pageSize = 20) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await service.GetPendingForProviderAsync(userId, page, pageSize);
            if (result.TotalCount == 0)
            {
                return Results.NotFound(new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND"));
            }
            return Results.Ok(result);
        })
        .WithName("GetHomeServiceProviderPendingBookings")
        .WithSummary("Get pending booking requests for the authenticated provider")
        .Produces<PagedResult<HomeServiceBookingResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
