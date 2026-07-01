using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEndPets.API.Endpoints;

public static class HomeProviderServiceAreasEndpoints
{
    public static IEndpointRouteBuilder MapHomeProviderServiceAreasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home-services/providers/me/service-areas")
            .WithTags("HomeServices")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext ctx, IHomeProviderServiceAreaService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetMyServiceAreasAsync(userId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyHomeProviderServiceAreas")
        .WithSummary("Get the authenticated provider's service areas")
        .Produces<IReadOnlyCollection<HomeProviderServiceAreaResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/", async (
            HttpContext ctx,
            [FromBody] IEnumerable<HomeProviderServiceAreaRequest> areas,
            IHomeProviderServiceAreaService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.ReplaceMyServiceAreasAsync(userId, areas);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "PROVIDER_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved providers can manage service areas.", "PROVIDER_NOT_APPROVED")),
                "INVALID_RADIUS" => Results.BadRequest(
                    new ApiErrorResponse("radiusKm must be greater than 0.", "INVALID_RADIUS")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ReplaceMyHomeProviderServiceAreas")
        .WithSummary("Replace the authenticated provider's full list of service areas")
        .Produces<IReadOnlyCollection<HomeProviderServiceAreaResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }
}
