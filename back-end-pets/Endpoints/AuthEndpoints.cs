using BackEndPets.Application.DTOs.Auth;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

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
