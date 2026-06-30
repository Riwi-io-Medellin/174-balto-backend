using System.Security.Claims;
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

        // ── Register ──────────────────────────────────────────────────────────
        group.MapPost("/register", async (RegisterRequest request, IAuthService service) =>
        {
            var result = await service.RegisterAsync(request);
            if (result.Tokens is not null)
                return Results.Created("/api/auth/register", result.Tokens);

            var error = new ApiErrorResponse(
                result.ErrorMessage ?? "Unable to register user.",
                result.ErrorCode    ?? "REGISTRATION_FAILED");

            return result.ErrorCode switch
            {
                "EMAIL_ALREADY_TAKEN" => Results.Conflict(error),
                _                     => Results.BadRequest(error)
            };
        })
        .AllowAnonymous()
        .WithName("Register")
        .WithSummary("Create an account and get tokens")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // ── Login ─────────────────────────────────────────────────────────────
        group.MapPost("/login", async (LoginRequest request, IAuthService service) =>
        {
            var (tokens, errorCode) = await service.LoginAsync(request);
        
            if (tokens is not null)
                return Results.Ok(tokens);
        
            var error = new ApiErrorResponse(
                errorCode switch
                {
                    "INVALID_CREDENTIALS" => "Email or password is incorrect.",
                    "VALIDATION_FAILED"   => "Email and password are required.",
                    _                     => "Unable to log in."
                },
                errorCode ?? "LOGIN_FAILED");
        
            return errorCode switch
            {
                "INVALID_CREDENTIALS" => Results.Json(error, statusCode: StatusCodes.Status401Unauthorized),
                "VALIDATION_FAILED"   => Results.BadRequest(error),
                _                     => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Log in and get tokens")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        // ── Refresh ───────────────────────────────────────────────────────────
        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService service) =>
        {
            var auth = await service.RefreshAsync(request);
            return auth is null
                ? Results.Unauthorized()
                : Results.Ok(auth);
        })
        .AllowAnonymous()
        .WithName("RefreshToken")
        .WithSummary("Refresh the access token")
        .Produces<AuthResponse>()
        .Produces(StatusCodes.Status401Unauthorized);

        // ── Logout ────────────────────────────────────────────────────────────
        group.MapPost("/logout", async (LogoutRequest request, IAuthService service) =>
        {
            var loggedOut = await service.LogoutAsync(request);
            return loggedOut ? Results.NoContent() : Results.NotFound();
        })
        .AllowAnonymous()
        .WithName("Logout")
        .WithSummary("Revoke a refresh token")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // ── Forgot password ───────────────────────────────────────────────────
        group.MapPost("/forgot-password", async (ForgotPasswordRequest request, IAuthService service) =>
        {
            // Siempre 200 para no revelar si el email existe.
            await service.ForgotPasswordAsync(request);
            return Results.Ok(new { Message = "If this email is registered, you will receive a password reset link." });
        })
        .AllowAnonymous()
        .WithName("ForgotPassword")
        .WithSummary("Request a password reset link (sent by email)")
        .Produces(StatusCodes.Status200OK);

        // ── Reset password ────────────────────────────────────────────────────
        group.MapPost("/reset-password", async (ResetPasswordRequest request, IAuthService service) =>
        {
            var (success, errorCode) = await service.ResetPasswordAsync(request);
            if (success) return Results.NoContent();

            var error = new ApiErrorResponse(
                errorCode switch
                {
                    "INVALID_TOKEN"    => "The reset token is invalid or has expired.",
                    "USER_NOT_FOUND"   => "Invalid request.",
                    "PASSWORD_WEAK"    => "Password does not meet the security requirements.",
                    "VALIDATION_FAILED"=> "All fields are required.",
                    _                  => "Unable to reset password."
                },
                errorCode ?? "RESET_FAILED");

            return errorCode switch
            {
                "INVALID_TOKEN"  => Results.UnprocessableEntity(error),
                _                => Results.BadRequest(error)
            };
        })
        .AllowAnonymous()
        .WithName("ResetPassword")
        .WithSummary("Set a new password using the reset token")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        // ── Change password (authenticated) ───────────────────────────────────
        group.MapPost("/change-password", async (
            ChangePasswordRequest request,
            ClaimsPrincipal principal,
            IAuthService service) =>
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.ChangePasswordAsync(userId, request);
            if (success) return Results.NoContent();

            var error = new ApiErrorResponse(
                errorCode switch
                {
                    "WRONG_PASSWORD"   => "Current password is incorrect.",
                    "PASSWORD_WEAK"    => "New password does not meet the security requirements.",
                    "VALIDATION_FAILED"=> "All fields are required.",
                    "USER_NOT_FOUND"   => "User not found.",
                    _                  => "Unable to change password."
                },
                errorCode ?? "CHANGE_FAILED");

            return errorCode switch
            {
                "WRONG_PASSWORD" => Results.UnprocessableEntity(error),
                _                => Results.BadRequest(error)
            };
        })
        .RequireAuthorization()
        .WithName("ChangePassword")
        .WithSummary("Change password using current password (requires authentication)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status401Unauthorized);

        // ── Google login ──────────────────────────────────────────────────────
        group.MapPost("/social/google", async (SocialLoginRequest request, IAuthService service) =>
        {
            var (tokens, errorCode) = await service.GoogleLoginAsync(request);
            if (tokens is not null) return Results.Ok(tokens);

            var error = new ApiErrorResponse(
                errorCode switch
                {
                    "INVALID_TOKEN"      => "The Google id_token is invalid or has expired.",
                    "EMAIL_REQUIRED"     => "Google account must have an email address.",
                    "REGISTRATION_FAILED"=> "Unable to create account.",
                    "VALIDATION_FAILED"  => "id_token is required.",
                    _                    => "Google authentication failed."
                },
                errorCode ?? "GOOGLE_AUTH_FAILED");

            return errorCode switch
            {
                "INVALID_TOKEN" => Results.Unauthorized(),
                _               => Results.BadRequest(error)
            };
        })
        .AllowAnonymous()
        .WithName("GoogleLogin")
        .WithSummary("Authenticate with a Google id_token from the native SDK")
        .Produces<AuthResponse>()
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // ── Apple login ───────────────────────────────────────────────────────
        group.MapPost("/social/apple", async (SocialLoginRequest request, IAuthService service) =>
        {
            var (tokens, errorCode) = await service.AppleLoginAsync(request);
            if (tokens is not null) return Results.Ok(tokens);

            var error = new ApiErrorResponse(
                errorCode switch
                {
                    "INVALID_TOKEN"      => "The Apple id_token is invalid or has expired.",
                    "EMAIL_REQUIRED"     => "Apple account must share an email address.",
                    "REGISTRATION_FAILED"=> "Unable to create account.",
                    "VALIDATION_FAILED"  => "id_token is required.",
                    _                    => "Apple authentication failed."
                },
                errorCode ?? "APPLE_AUTH_FAILED");

            return errorCode switch
            {
                "INVALID_TOKEN" => Results.Unauthorized(),
                _               => Results.BadRequest(error)
            };
        })
        .AllowAnonymous()
        .WithName("AppleLogin")
        .WithSummary("Authenticate with an Apple id_token from the native SDK")
        .Produces<AuthResponse>()
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
