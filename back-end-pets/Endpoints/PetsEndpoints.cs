using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class PetsEndpoints
{
    public static IEndpointRouteBuilder MapPetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets")
            .WithTags("Pets")
            .RequireAuthorization();

        group.MapPost("/", async (CreatePetRequest request, HttpContext ctx, IPetService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var pet = await service.CreateAsync(userId, request);
            return Results.Created($"/api/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Register a new pet for the current user")
        .Produces<PetResponse>(StatusCodes.Status201Created);

        group.MapGet("/me", async (
            int page,
            int pageSize,
            HttpContext ctx,
            IPetService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await service.GetByUserIdAsync(userId, page, pageSize);
            return Results.Ok(result);
        })
        .WithName("GetMyPets")
        .WithSummary("Get all pets of the current user")
        .Produces<PagedResult<PetResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IPetService service) =>
        {
            var pet = await service.GetByIdAsync(id);
            return pet is null
                ? Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND"))
                : Results.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("Get a pet by id")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdatePetRequest request, HttpContext ctx, IPetService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (pet, errorCode) = await service.UpdateAsync(userId, id, request);
            return errorCode switch
            {
                "PET_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(new ApiErrorResponse("You do not own this pet.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.Ok(pet)
            };
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet owned by the current user")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext ctx, IPetService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteAsync(userId, id);
            return errorCode switch
            {
                "PET_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(new ApiErrorResponse("You do not own this pet.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeletePet")
        .WithSummary("Delete a pet owned by the current user")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}