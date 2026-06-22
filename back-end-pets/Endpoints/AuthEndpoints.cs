using BackEndPets.Application.DTOs.Auth;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService service) =>
        {
            var result = await service.RegisterAsync(request);
            if (result.Tokens is not null)
            {
                return Results.Created("/api/auth/register", result.Tokens);
            }

            var error = new ApiErrorResponse(
                result.ErrorMessage ?? "Unable to register user.",
                result.ErrorCode ?? "REGISTRATION_FAILED");

            return result.ErrorCode switch
            {
                "EMAIL_ALREADY_TAKEN" => Results.Conflict(error),
                _ => Results.BadRequest(error)
            };
        })
        .AllowAnonymous()
        .WithName("Register")
        .WithSummary("Create an account and get tokens")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginRequest request, IAuthService service) =>
        {
            var auth = await service.LoginAsync(request);
            return auth is null
                ? Results.Unauthorized()
                : Results.Ok(auth);
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Log in and get tokens");

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService service) =>
        {
            var auth = await service.RefreshAsync(request);
            return auth is null
                ? Results.Unauthorized()
                : Results.Ok(auth);
        })
        .AllowAnonymous()
        .WithName("RefreshToken")
        .WithSummary("Refresh the access token");

        group.MapPost("/logout", async (LogoutRequest request, IAuthService service) =>
        {
            var loggedOut = await service.LogoutAsync(request);
            return loggedOut ? Results.NoContent() : Results.NotFound();
        })
        .AllowAnonymous()
        .WithName("Logout")
        .WithSummary("Revoke a refresh token");

        return app;
    }
}
