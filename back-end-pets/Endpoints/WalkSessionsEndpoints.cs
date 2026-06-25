using System.Security.Claims;
using BackEndPets.API.Hubs;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.WalkSessions;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BackEndPets.API.Endpoints;

public static class WalkSessionsEndpoints
{
    public static IEndpointRouteBuilder MapWalkSessionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/walk-sessions")
            .WithTags("WalkSessions")
            .RequireAuthorization();

        group.MapPost("/", async (
            StartWalkSessionRequest request,
            HttpContext ctx,
            IWalkSessionService service,
            IHubContext<WalkTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (session, errorCode) = await service.StartSessionAsync(userId, request);
            if (session is not null)
            {
                await hub.Clients.Group($"walk-{session.Id}").SendAsync("WalkStarted", session);
                return Results.Created($"/api/walk-sessions/{session.Id}", session);
            }

            return errorCode switch
            {
                "HISTORY_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Pet walking history not found.", "HISTORY_NOT_FOUND")),
                "WALKER_NOT_FOUND" => Results.Json(
                    new ApiErrorResponse("You do not have a walker profile.", "WALKER_NOT_FOUND"),
                    statusCode: StatusCodes.Status403Forbidden),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You are not assigned to this walk.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("StartWalkSession")
        .WithSummary("Start a new walk session (assigned walker only)")
        .Produces<WalkSessionResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{sessionId:guid}/location", async (
            Guid sessionId,
            AddLocationRequest request,
            HttpContext ctx,
            IWalkSessionService service,
            IHubContext<WalkTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (point, errorCode) = await service.AddLocationAsync(userId, sessionId, request);
            if (point is not null)
            {
                var payload = new LocationUpdatedPayload(sessionId, point.Latitude, point.Longitude, point.CreatedAt);
                await hub.Clients.Group($"walk-{sessionId}").SendAsync("WalkLocationUpdated", payload);
                return Results.Ok(point);
            }

            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You are not the walker for this session.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_STATUS_TRANSITION" => Results.Conflict(
                    new ApiErrorResponse("Session is not in progress.", "INVALID_STATUS_TRANSITION")),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("AddWalkLocation")
        .WithSummary("Add a GPS point and broadcast to connected clients")
        .Produces<WalkRoutePointResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/{sessionId:guid}/pause", async (
            Guid sessionId,
            HttpContext ctx,
            IWalkSessionService service,
            IHubContext<WalkTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (session, errorCode) = await service.PauseSessionAsync(userId, sessionId);
            if (session is not null)
            {
                await hub.Clients.Group($"walk-{sessionId}").SendAsync("WalkPaused", session);
                return Results.Ok(session);
            }

            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You are not the walker for this session.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_STATUS_TRANSITION" => Results.Conflict(
                    new ApiErrorResponse("Session is not in progress.", "INVALID_STATUS_TRANSITION")),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("PauseWalkSession")
        .WithSummary("Pause an in-progress walk session")
        .Produces<WalkSessionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/{sessionId:guid}/resume", async (
            Guid sessionId,
            HttpContext ctx,
            IWalkSessionService service,
            IHubContext<WalkTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (session, errorCode) = await service.ResumeSessionAsync(userId, sessionId);
            if (session is not null)
            {
                await hub.Clients.Group($"walk-{sessionId}").SendAsync("WalkResumed", session);
                return Results.Ok(session);
            }

            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You are not the walker for this session.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_STATUS_TRANSITION" => Results.Conflict(
                    new ApiErrorResponse("Session is not paused.", "INVALID_STATUS_TRANSITION")),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("ResumeWalkSession")
        .WithSummary("Resume a paused walk session")
        .Produces<WalkSessionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/{sessionId:guid}/complete", async (
            Guid sessionId,
            HttpContext ctx,
            IWalkSessionService service,
            IHubContext<WalkTrackingHub> hub) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (session, errorCode) = await service.CompleteSessionAsync(userId, sessionId);
            if (session is not null)
            {
                await hub.Clients.Group($"walk-{sessionId}").SendAsync("WalkCompleted", session);
                return Results.Ok(session);
            }

            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You are not the walker for this session.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_STATUS_TRANSITION" => Results.Conflict(
                    new ApiErrorResponse("Session cannot be completed from its current state.", "INVALID_STATUS_TRANSITION")),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CompleteWalkSession")
        .WithSummary("Complete a walk session")
        .Produces<WalkSessionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/{sessionId:guid}/route", async (
            Guid sessionId,
            HttpContext ctx,
            IWalkSessionService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (points, errorCode) = await service.GetRouteAsync(userId, sessionId);
            if (points is not null)
                return Results.Ok(points);

            return errorCode switch
            {
                "SESSION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Session not found.", "SESSION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not have access to this session.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("GetWalkRoute")
        .WithSummary("Get all GPS route points for a session (walker or pet owner)")
        .Produces<IReadOnlyCollection<WalkRoutePointResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
