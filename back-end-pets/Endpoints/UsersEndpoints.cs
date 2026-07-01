using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Users;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", async (IUserService service) => Results.Ok(await service.GetAllAsync()))
            .WithName("GetUsers")
            .WithSummary("List users")
            .RequireAuthorization("AdminOnly")
            .Produces<IReadOnlyCollection<UserResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IUserService service) =>
        {
            var user = await service.GetByIdAsync(id);
            return user is null
                ? Results.NotFound(new ApiErrorResponse("User not found.", "USER_NOT_FOUND"))
                : Results.Ok(user);
        })
        .WithName("GetUserById")
        .WithSummary("Get a user by id")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserRequest request, IUserService service) =>
        {
            var user = await service.CreateAsync(request);
            return user is null
                ? Results.BadRequest(new ApiErrorResponse("Unable to create user.", "USER_CREATE_FAILED"))
                : Results.Created($"/api/users/{user.Id}", user);
        })
        .WithName("CreateUser")
        .WithSummary("Create a user")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IUserService service) =>
        {
            var user = await service.UpdateAsync(id, request);
            return user is null
                ? Results.NotFound(new ApiErrorResponse("User not found or request is invalid.", "USER_UPDATE_FAILED"))
                : Results.Ok(user);
        })
        .WithName("UpdateUser")
        .WithSummary("Update a user")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IUserService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new ApiErrorResponse("User not found.", "USER_NOT_FOUND"));
        })
        .WithName("DeleteUser")
        .WithSummary("Delete a user")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
