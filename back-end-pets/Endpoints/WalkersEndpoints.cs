using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Profiles;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class WalkersEndpoints
{
    public static IEndpointRouteBuilder MapWalkersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/walkers")
            .WithTags("Walkers")
            .RequireAuthorization();

        group.MapPost("/", async (HttpContext ctx, IProfileService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (walker, errorCode) = await service.BecomeWalkerAsync(userId);
            return errorCode switch
            {
                "WALKER_ALREADY_EXISTS" => Results.Conflict(
                    new ApiErrorResponse("User is already registered as a walker.", "WALKER_ALREADY_EXISTS")),
                _ when walker is not null => Results.Created($"/api/walkers/{walker.Id}", walker),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("BecomeWalker")
        .WithSummary("Register the current user as a walker")
        .Produces<WalkerResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/", async (
                    [AsParameters] WalkerFilterRequest filters,
                    IProfileService service) =>
                Results.Ok(await service.GetWalkersAsync(filters.Available, filters.WorkLocation)))
            .WithName("GetWalkers")
            .WithSummary("List all walkers")
            .Produces<IReadOnlyCollection<WalkerResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IProfileService service) =>
            {
                var walker = await service.GetWalkerByIdAsync(id);
                return walker is null
                    ? Results.NotFound(new ApiErrorResponse("Walker not found.", "WALKER_NOT_FOUND"))
                    : Results.Ok(walker);
            })
            .WithName("GetWalkerById")
            .WithSummary("Get a walker by id")
            .Produces<WalkerResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
        
        return app;
    }
}
