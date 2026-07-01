using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEndPets.API.Endpoints;

public static class HomeProviderAvailabilityEndpoints
{
    public static IEndpointRouteBuilder MapHomeProviderAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home-services/providers/me/availability")
            .WithTags("HomeServices")
            .RequireAuthorization();

        // ── Weekly availability ───────────────────────────────────────────────

        group.MapGet("/", async (HttpContext ctx, IHomeProviderAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetMyAvailabilityAsync(userId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyHomeProviderAvailability")
        .WithSummary("Get the authenticated provider's weekly availability")
        .Produces<IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/", async (
            HttpContext ctx,
            [FromBody] IEnumerable<HomeProviderAvailabilitySlotRequest> slots,
            IHomeProviderAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.ReplaceMyAvailabilityAsync(userId, slots);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "PROVIDER_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved providers can manage availability.", "PROVIDER_NOT_APPROVED")),
                "INVALID_DAY_OF_WEEK" => Results.BadRequest(
                    new ApiErrorResponse("dayOfWeek must be between 0 (Sunday) and 6 (Saturday).", "INVALID_DAY_OF_WEEK")),
                "INVALID_TIME_RANGE" => Results.BadRequest(
                    new ApiErrorResponse("startTime must be earlier than endTime.", "INVALID_TIME_RANGE")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ReplaceMyHomeProviderAvailability")
        .WithSummary("Replace the authenticated provider's full weekly availability")
        .Produces<IReadOnlyCollection<HomeProviderAvailabilitySlotResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // ── Date exceptions ───────────────────────────────────────────────────

        group.MapGet("/exceptions", async (HttpContext ctx, IHomeProviderAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetMyExceptionsAsync(userId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyHomeProviderAvailabilityExceptions")
        .WithSummary("Get the authenticated provider's availability exceptions")
        .Produces<IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/exceptions", async (
            HttpContext ctx,
            [FromBody] IEnumerable<HomeProviderAvailabilityExceptionRequest> exceptions,
            IHomeProviderAvailabilityService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.ReplaceMyExceptionsAsync(userId, exceptions);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "PROVIDER_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved providers can manage availability.", "PROVIDER_NOT_APPROVED")),
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
        .WithName("ReplaceMyHomeProviderAvailabilityExceptions")
        .WithSummary("Replace the authenticated provider's full list of availability exceptions")
        .Produces<IReadOnlyCollection<HomeProviderAvailabilityExceptionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }
}
