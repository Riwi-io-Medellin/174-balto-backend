using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Profiles;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me", async (HttpContext ctx, IProfileService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var me = await service.GetMeAsync(userId);
            return me is null
                ? Results.NotFound(new ApiErrorResponse("User not found.", "USER_NOT_FOUND"))
                : Results.Ok(me);
        })
        .WithTags("Me")
        .WithName("GetMe")
        .WithSummary("Get the authenticated user's aggregated profile")
        .RequireAuthorization()
        .Produces<MeResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
