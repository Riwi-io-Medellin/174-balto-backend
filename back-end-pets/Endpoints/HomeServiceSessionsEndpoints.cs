using System.Security.Claims;
using BackEndPets.API.Hubs;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BackEndPets.API.Endpoints;

public static class HomeServiceSessionsEndpoints
{
    public static IEndpointRouteBuilder MapHomeServiceSessionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home-service-sessions")
            .WithTags("HomeServiceSessions")
            .RequireAuthorization();

        group.MapPost("/from-booking/{bookingId:guid}", async (
            Guid bookingId,
            HttpContext ctx,
            IHomeServiceSessionService service,
            IHubContext<HomeServiceTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (session, errorCode) = await service.StartFromBookingAsync(userId, bookingId);
            if (session is not null)
            {
                await hub.Clients.Group($"home-service-{session.Id}").SendAsync("HomeServiceStarted", session);
                return Results.Created($"/api/home-service-sessions/{session.Id}", session);
            }

            return errorCode switch
            {
                "PROVIDER_NOT_FOUND"              => Results.Json(new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_NOT_FOUND"                => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"                      => Results.Json(new ApiErrorResponse("Booking is not assigned to you.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "BOOKING_NOT_ACCEPTED"              => Results.Conflict(new ApiErrorResponse("Booking is not in accepted status.", "BOOKING_NOT_ACCEPTED")),
                "PROVIDER_HAS_ACTIVE_SESSION"       => Results.Conflict(new ApiErrorResponse("You already have an active session.", "PROVIDER_HAS_ACTIVE_SESSION")),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("StartHomeServiceSessionFromBooking")
        .WithSummary("Start a home service session from an accepted booking (provider only)")
        .Produces<HomeServiceSessionResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/{sessionId:guid}/events", async (
            Guid sessionId,
            AddHomeServiceSessionEventRequest request,
            HttpContext ctx,
            IHomeServiceSessionService service,
            IHubContext<HomeServiceTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (sessionEvent, errorCode) = await service.AddEventAsync(userId, sessionId, request);
            if (sessionEvent is not null)
            {
                await hub.Clients.Group($"home-service-{sessionId}").SendAsync("HomeServiceEventAdded", sessionEvent);
                return Results.Ok(sessionEvent);
            }

            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You are not the provider for this session.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_STATUS_TRANSITION" => Results.Conflict(
                    new ApiErrorResponse("Session is not in progress.", "INVALID_STATUS_TRANSITION")),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddHomeServiceSessionEvent")
        .WithSummary("Add a timeline event to an in-progress session and broadcast to connected clients")
        .Produces<HomeServiceSessionEventResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/{sessionId:guid}/finish", async (
            Guid sessionId,
            FinishHomeServiceSessionRequest request,
            HttpContext ctx,
            IHomeServiceSessionService service,
            IHubContext<HomeServiceTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (session, errorCode) = await service.FinishAsync(userId, sessionId, request);
            if (session is not null)
            {
                await hub.Clients.Group($"home-service-{sessionId}").SendAsync("HomeServiceFinished", session);
                return Results.Ok(session);
            }

            return errorCode switch
            {
                "PROVIDER_NOT_FOUND"        => Results.Json(new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND"), statusCode: StatusCodes.Status403Forbidden),
                "SESSION_NOT_FOUND"         => Results.NotFound(new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED"              => Results.Json(new ApiErrorResponse("Session is not yours.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "SESSION_NOT_ACTIVE"        => Results.Conflict(new ApiErrorResponse("Session is not in progress.", "SESSION_NOT_ACTIVE")),
                "SESSION_NOT_BOOKING_BASED" => Results.Conflict(new ApiErrorResponse("Session was not started from a booking.", "SESSION_NOT_BOOKING_BASED")),
                "BOOKING_NOT_FOUND"         => Results.NotFound(new ApiErrorResponse("Linked booking not found.", "BOOKING_NOT_FOUND")),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("FinishHomeServiceSession")
        .WithSummary("Finish an active session and complete the linked booking (provider only)")
        .Produces<HomeServiceSessionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/{sessionId:guid}", async (
            Guid sessionId,
            IHomeServiceSessionService service) =>
        {
            var (session, errorCode) = await service.GetSessionDetailAsync(sessionId);
            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                _ when session is not null => Results.Ok(session),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetHomeServiceSessionById")
        .WithSummary("Get full session details by ID")
        .Produces<HomeServiceSessionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{sessionId:guid}/events", async (
            Guid sessionId,
            HttpContext ctx,
            IHomeServiceSessionService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (events, errorCode) = await service.GetEventsAsync(userId, sessionId);
            if (events is not null)
                return Results.Ok(events);

            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not have access to this session.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetHomeServiceSessionEvents")
        .WithSummary("Get the timeline of events for a session (provider or client)")
        .Produces<IReadOnlyCollection<HomeServiceSessionEventResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        var providers = app.MapGroup("/api/home-services/providers")
            .WithTags("HomeServices")
            .RequireAuthorization();

        providers.MapGet("/me/active-session", async (
            HttpContext ctx,
            IHomeServiceSessionService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (session, errorCode) = await service.GetActiveForProviderAsync(userId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "NO_ACTIVE_SESSION"  => Results.NotFound(new ApiErrorResponse("No active session.", "NO_ACTIVE_SESSION")),
                _ when session is not null => Results.Ok(session),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetActiveHomeServiceSession")
        .WithSummary("Get the authenticated provider's current active session")
        .Produces<HomeServiceSessionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
