using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class PetsEndpoints
{
    public static IEndpointRouteBuilder MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users/{userId:guid}/pets")
            .WithTags("Pets")
            .RequireAuthorization();

        group.MapGet("/", async (Guid userId, IPetService service) => Results.Ok(await service.GetByUserIdAsync(userId)))
            .WithName("GetPetsByUser")
            .WithSummary("List pets for a user")
            .Produces<IReadOnlyCollection<PetResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid userId, Guid id, IPetService service) =>
        {
            var pet = await service.GetByIdAsync(userId, id);
            return pet is null
                ? Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND"))
                : Results.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("Get a pet by id")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (Guid userId, CreatePetRequest request, IPetService service) =>
        {
            var pet = await service.CreateAsync(userId, request);
            return pet is null
                ? Results.BadRequest(new ApiErrorResponse("Unable to create pet.", "PET_CREATE_FAILED"))
                : Results.Created($"/api/users/{userId}/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Create a pet for a user")
        .Produces<PetResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid userId, Guid id, UpdatePetRequest request, IPetService service) =>
        {
            var pet = await service.UpdateAsync(userId, id, request);
            return pet is null
                ? Results.NotFound(new ApiErrorResponse("Pet not found or request is invalid.", "PET_UPDATE_FAILED"))
                : Results.Ok(pet);
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid userId, Guid id, IPetService service) =>
        {
            var deleted = await service.DeleteAsync(userId, id);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND"));
        })
        .WithName("DeletePet")
        .WithSummary("Delete a pet")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
