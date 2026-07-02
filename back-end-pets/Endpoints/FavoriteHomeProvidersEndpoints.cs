using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class FavoriteHomeProvidersEndpoints
{
    public static IEndpointRouteBuilder MapFavoriteHomeProvidersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home-services/favorites")
            .WithTags("HomeServices")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext ctx, IFavoriteHomeProviderService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            return Results.Ok(await service.GetMyFavoritesAsync(userId));
        })
        .WithName("GetMyFavoriteHomeProviders")
        .WithSummary("Get the authenticated user's favorited home service providers")
        .Produces<IReadOnlyCollection<FavoriteHomeProviderResponse>>(StatusCodes.Status200OK);

        group.MapPost("/{providerId:guid}", async (
            Guid providerId,
            HttpContext ctx,
            IFavoriteHomeProviderService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.AddFavoriteAsync(userId, providerId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider not found.", "PROVIDER_NOT_FOUND")),
                "ALREADY_FAVORITED" => Results.Conflict(
                    new ApiErrorResponse("You already favorited this provider.", "ALREADY_FAVORITED")),
                _ when success => Results.Created($"/api/home-services/favorites/{providerId}", providerId),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddFavoriteHomeProvider")
        .WithSummary("Add a home service provider to the authenticated user's favorites")
        .Produces(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapDelete("/{providerId:guid}", async (
            Guid providerId,
            HttpContext ctx,
            IFavoriteHomeProviderService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.RemoveFavoriteAsync(userId, providerId);
            return errorCode switch
            {
                "NOT_FAVORITED" => Results.NotFound(
                    new ApiErrorResponse("This provider is not in your favorites.", "NOT_FAVORITED")),
                _ when success => Results.NoContent(),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("RemoveFavoriteHomeProvider")
        .WithSummary("Remove a home service provider from the authenticated user's favorites")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
