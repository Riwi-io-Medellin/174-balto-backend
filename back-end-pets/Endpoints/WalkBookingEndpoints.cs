using System.Security.Claims;
using BackEndPets.Application.DTOs.Bookings;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class WalkBookingEndpoints
{
    public static IEndpointRouteBuilder MapWalkBookingEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Client endpoints ──────────────────────────────────────────────────

        var bookings = app.MapGroup("/api/walk-bookings")
            .WithTags("WalkBookings")
            .RequireAuthorization();

        bookings.MapPost("/", async (
            CreateBookingRequest request,
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.CreateAsync(userId, request);
            return errorCode switch
            {
                "INVALID_DURATION"              => Results.BadRequest(new ApiErrorResponse("durationMinutes must be 30, 60, or 90.", "INVALID_DURATION")),
                "PET_NOT_FOUND"                 => Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                "PET_NOT_OWNED"                 => Results.Json(new ApiErrorResponse("Pet does not belong to you.", "PET_NOT_OWNED"), statusCode: StatusCodes.Status403Forbidden),
                "WALKER_NOT_FOUND"              => Results.NotFound(new ApiErrorResponse("Walker not found.", "WALKER_NOT_FOUND")),
                "WALKER_NOT_APPROVED"           => Results.Conflict(new ApiErrorResponse("Walker is not approved.", "WALKER_NOT_APPROVED")),
                "WALKER_NOT_ACCEPTING_BOOKINGS" => Results.Conflict(new ApiErrorResponse("Walker is not accepting bookings.", "WALKER_NOT_ACCEPTING_BOOKINGS")),
                "SLOT_IN_PAST"                  => Results.BadRequest(new ApiErrorResponse("Slot start must be in the future.", "SLOT_IN_PAST")),
                "SLOT_NOT_AVAILABLE"            => Results.Conflict(new ApiErrorResponse("Requested slot is not available for this walker.", "SLOT_NOT_AVAILABLE")),
                _ when result is not null       => Results.Created($"/api/walk-bookings/{result.Id}", result),
                _                               => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CreateBooking")
        .WithSummary("Create a new walk booking")
        .Produces<BookingResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapGet("/me", async (
            string? status,
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            return Results.Ok(await service.GetMyBookingsAsync(userId, status));
        })
        .WithName("GetMyBookings")
        .WithSummary("Get the authenticated user's bookings")
        .Produces<IReadOnlyCollection<BookingResponse>>(StatusCodes.Status200OK);

        bookings.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext ctx,
            IWalkBookingService service) =>
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
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("GetBookingById")
        .WithSummary("Get booking details (owner or assigned walker only)")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        bookings.MapPost("/{id:guid}/accept", async (
            Guid id,
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.AcceptAsync(userId, id);
            return errorCode switch
            {
                "WALKER_NOT_FOUND"  => Results.NotFound(new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                "BOOKING_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"      => Results.Json(new ApiErrorResponse("This booking is not assigned to you.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_NOT_PENDING" => Results.Conflict(new ApiErrorResponse("Booking is not in pending status.", "BOOKING_NOT_PENDING")),
                _ when result is not null => Results.Ok(result),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("AcceptBooking")
        .WithSummary("Accept a pending booking (walker only)")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapPost("/{id:guid}/reject", async (
            Guid id,
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.RejectAsync(userId, id);
            return errorCode switch
            {
                "WALKER_NOT_FOUND"         => Results.NotFound(new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                "BOOKING_NOT_FOUND"        => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"             => Results.Json(new ApiErrorResponse("This booking is not assigned to you.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "CANNOT_REJECT_COMPLETED"  => Results.Conflict(new ApiErrorResponse("Cannot reject a completed booking.", "CANNOT_REJECT_COMPLETED")),
                "BOOKING_ALREADY_RESOLVED" => Results.Conflict(new ApiErrorResponse("Booking is already in a terminal state.", "BOOKING_ALREADY_RESOLVED")),
                _ when result is not null  => Results.Ok(result),
                _                          => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("RejectBooking")
        .WithSummary("Reject a booking (walker only)")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapPost("/{id:guid}/cancel", async (
            Guid id,
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.CancelByWalkerAsync(userId, id);
            return errorCode switch
            {
                "WALKER_NOT_FOUND"         => Results.NotFound(new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                "BOOKING_NOT_FOUND"        => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"             => Results.Json(new ApiErrorResponse("This booking is not assigned to you.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_ALREADY_RESOLVED" => Results.Conflict(new ApiErrorResponse("Booking is already in a terminal state.", "BOOKING_ALREADY_RESOLVED")),
                "CANNOT_CANCEL_COMPLETED"  => Results.Conflict(new ApiErrorResponse("Cannot cancel a completed booking.", "CANNOT_CANCEL_COMPLETED")),
                _ when result is not null  => Results.Ok(result),
                _                          => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CancelBookingByWalker")
        .WithSummary("Cancel a booking as the assigned walker (sets status to walker_cancelled)")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        bookings.MapPost("/{id:guid}/owner-cancel", async (
            Guid id,
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.CancelByOwnerAsync(userId, id);
            return errorCode switch
            {
                "BOOKING_NOT_FOUND"        => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"             => Results.Json(new ApiErrorResponse("You do not own this booking.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_ALREADY_RESOLVED" => Results.Conflict(new ApiErrorResponse("Booking is already in a terminal state.", "BOOKING_ALREADY_RESOLVED")),
                "CANNOT_CANCEL_COMPLETED"  => Results.Conflict(new ApiErrorResponse("Cannot cancel a completed booking.", "CANNOT_CANCEL_COMPLETED")),
                _ when result is not null  => Results.Ok(result),
                _                          => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CancelBookingByOwner")
        .WithSummary("Cancel a booking as the booking owner (sets status to owner_cancelled)")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // ── Walker-facing endpoint ────────────────────────────────────────────

        var walkers = app.MapGroup("/api/walkers")
            .WithTags("Walkers")
            .RequireAuthorization();

        walkers.MapGet("/me/bookings", async (
            string? status,
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetWalkerBookingsAsync(userId, status);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("GetWalkerBookings")
        .WithSummary("Get all bookings for the authenticated walker (optional status filter)")
        .Produces<IReadOnlyCollection<BookingResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        walkers.MapGet("/me/pending-bookings", async (
            HttpContext ctx,
            IWalkBookingService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetPendingForWalkerAsync(userId);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("GetPendingBookings")
        .WithSummary("Get pending booking requests for the authenticated walker")
        .Produces<IReadOnlyCollection<BookingResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
