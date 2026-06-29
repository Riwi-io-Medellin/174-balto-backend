using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.WalkingHistory;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class WalkingHistoryEndpoints
{
    public static IEndpointRouteBuilder MapWalkingHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/walking-history")
            .WithTags("WalkingHistory")
            .RequireAuthorization();

        group.MapPost("/", async (
            CreateWalkingHistoryRequest request,
            HttpContext ctx,
            IWalkingHistoryService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (history, errorCode) = await service.CreateAsync(userId, request);
            return errorCode switch
            {
                "PET_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                "PET_NOT_OWNED" => Results.Json(
                    new ApiErrorResponse("You do not own this pet.", "PET_NOT_OWNED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker not found.", "WALKER_NOT_FOUND")),
                _ when history is not null => Results.Created($"/api/walking-history/{history.Id}", history),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CreateWalkingHistory")
        .WithSummary("Create a walking history entry for a pet")
        .Produces<WalkingHistoryResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/me", async (
            int page,
            int pageSize,
            HttpContext ctx,
            IWalkingHistoryService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await service.GetMyHistoryAsync(userId, page, pageSize);
            return Results.Ok(result);
        })
        .WithName("GetMyWalkingHistory")
        .WithSummary("Get all walking history entries for the current user")
        .Produces<PagedResult<WalkingHistoryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/walker", async (
            int page,
            int pageSize,
            HttpContext ctx,
            IWalkingHistoryService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await service.GetByWalkerAsync(userId, page, pageSize);
            return Results.Ok(result);
        })
        .WithName("GetWalkerHistory")
        .WithSummary("Get all walking history entries assigned to the current walker")
        .Produces<PagedResult<WalkingHistoryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, HttpContext ctx, IWalkingHistoryService service) =>
        {
            var history = await service.GetByIdAsync(id);
            return history is null
                ? Results.NotFound(new ApiErrorResponse("Walking history not found.", "HISTORY_NOT_FOUND"))
                : Results.Ok(history);
        })
        .WithName("GetWalkingHistoryById")
        .WithSummary("Get a walking history entry by id")
        .Produces<WalkingHistoryResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}