using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class PetHistoryEndpoints
{
    public static IEndpointRouteBuilder MapPetHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets")
            .WithTags("Pets")
            .RequireAuthorization();

        group.MapPost("/{petId:guid}/history", async (
            Guid petId,
            CreatePetHistoryRequest request,
            HttpContext ctx,
            IPetHistoryService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (history, errorCode) = await service.CreateAsync(userId, petId, request);
            return errorCode switch
            {
                "PET_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this pet.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ when history is not null => Results.Created(
                    $"/api/pets/{petId}/history/{history.Id}", history),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CreatePetHistory")
        .WithSummary("Add a history entry to a pet (owner only)")
        .Produces<PetHistoryResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{petId:guid}/history", async (
            Guid petId,
            int page,
            int pageSize,
            HttpContext ctx,
            IPetHistoryService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetByPetIdAsync(userId, petId, page, pageSize);
            return errorCode switch
            {
                "PET_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this pet.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.Ok(result)
            };
        })
        .WithName("GetPetHistory")
        .WithSummary("Get all history entries for a pet (owner only)")
        .Produces<PagedResult<PetHistoryResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{petId:guid}/history/{historyId:guid}", async (
            Guid petId,
            Guid historyId,
            HttpContext ctx,
            IPetHistoryService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteAsync(userId, historyId);
            return errorCode switch
            {
                "HISTORY_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("History entry not found.", "HISTORY_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this pet.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeletePetHistory")
        .WithSummary("Delete a history entry from a pet (owner only)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}