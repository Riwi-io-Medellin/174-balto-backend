using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class PetsEndpoints
{
    public static IEndpointRouteBuilder MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets")
            .WithTags("Pets");

        group.MapGet("/", async (IPetService service) => Results.Ok(await service.GetAllAsync()))
            .WithName("GetPets")
            .WithSummary("List pets")
            .Produces<IReadOnlyCollection<PetResponse>>(StatusCodes.Status200OK);

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

        group.MapPost("/", async (CreatePetRequest request, IPetService service) =>
        {
            var pet = await service.CreateAsync(request);
            return pet is null
                ? Results.BadRequest(new ApiErrorResponse("Unable to create pet.", "PET_CREATE_FAILED"))
                : Results.Created($"/api/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Create a pet")
        .Produces<PetResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdatePetRequest request, IPetService service) =>
        {
            var pet = await service.UpdateAsync(id, request);
            return pet is null
                ? Results.NotFound(new ApiErrorResponse("Pet not found or request is invalid.", "PET_UPDATE_FAILED"))
                : Results.Ok(pet);
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IPetService service) =>
        {
            var deleted = await service.DeleteAsync(id);
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
