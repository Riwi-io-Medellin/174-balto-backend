using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEndPets.API.Endpoints;

public static class WalkerAvailabilityEndpoints
{
    public static IEndpointRouteBuilder MapWalkerAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/walkers/me/availability")
            .WithTags("Walkers")
            .RequireAuthorization();

        // ── Weekly availability ───────────────────────────────────────────────

        group.MapGet("/", async (HttpContext ctx, IWalkerAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetMyAvailabilityAsync(userId);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyAvailability")
        .WithSummary("Get the authenticated walker's weekly availability")
        .Produces<IReadOnlyCollection<AvailabilitySlotResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/", async (
            HttpContext ctx,
            [FromBody] IEnumerable<AvailabilitySlotRequest> slots,
            IWalkerAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.ReplaceMyAvailabilityAsync(userId, slots);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                "WALKER_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved walkers can manage availability.", "WALKER_NOT_APPROVED")),
                "INVALID_DAY_OF_WEEK" => Results.BadRequest(
                    new ApiErrorResponse("dayOfWeek must be between 0 (Sunday) and 6 (Saturday).", "INVALID_DAY_OF_WEEK")),
                "INVALID_TIME_RANGE" => Results.BadRequest(
                    new ApiErrorResponse("startTime must be earlier than endTime.", "INVALID_TIME_RANGE")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ReplaceMyAvailability")
        .WithSummary("Replace the authenticated walker's full weekly availability")
        .Produces<IReadOnlyCollection<AvailabilitySlotResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // ── Date exceptions ───────────────────────────────────────────────────

        group.MapGet("/exceptions", async (HttpContext ctx, IWalkerAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetMyExceptionsAsync(userId);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyAvailabilityExceptions")
        .WithSummary("Get the authenticated walker's availability exceptions")
        .Produces<IReadOnlyCollection<AvailabilityExceptionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/exceptions", async (
            HttpContext ctx,
            [FromBody] IEnumerable<AvailabilityExceptionRequest> exceptions,
            IWalkerAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.ReplaceMyExceptionsAsync(userId, exceptions);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                "WALKER_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved walkers can manage availability.", "WALKER_NOT_APPROVED")),
                "DUPLICATE_DATE" => Results.BadRequest(
                    new ApiErrorResponse("Each date must appear only once in the list.", "DUPLICATE_DATE")),
                "TIMES_REQUIRED_WHEN_AVAILABLE" => Results.BadRequest(
                    new ApiErrorResponse(
                        "startTime and endTime are required when isUnavailable is false.",
                        "TIMES_REQUIRED_WHEN_AVAILABLE")),
                "INVALID_TIME_RANGE" => Results.BadRequest(
                    new ApiErrorResponse("startTime must be earlier than endTime.", "INVALID_TIME_RANGE")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ReplaceMyAvailabilityExceptions")
        .WithSummary("Replace the authenticated walker's full list of availability exceptions")
        .Produces<IReadOnlyCollection<AvailabilityExceptionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }
}
