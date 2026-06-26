using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackEndPets.Application.DTOs.Auth;
using BackEndPets.Application.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BackEndPets.Infrastructure.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private static readonly ConcurrentDictionary<string, RefreshTokenRecord> RefreshTokens = new();

    // ── Registro ─────────────────────────────────────────────────────────────

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        var firstName  = request.FirstName?.Trim()  ?? string.Empty;
        var lastName   = request.LastName?.Trim()   ?? string.Empty;
        var email      = request.Email?.Trim()      ?? string.Empty;
        var password   = request.Password           ?? string.Empty;
        var idNumber   = request.IdNumber?.Trim()   ?? string.Empty;
        var idType     = request.IdType?.Trim()     ?? string.Empty;
        var phone      = request.Phone?.Trim()      ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName)  ||
            string.IsNullOrWhiteSpace(lastName)   ||
            string.IsNullOrWhiteSpace(email)      ||
            string.IsNullOrWhiteSpace(password)   ||
            string.IsNullOrWhiteSpace(idNumber)   ||
            string.IsNullOrWhiteSpace(idType)     ||
            string.IsNullOrWhiteSpace(phone))
        {
            return new RegisterResult(null, "VALIDATION_FAILED", "All fields are required.");
        }

        string[] validIdTypes = ["CC", "CE", "Passport", "TI"];
        if (!validIdTypes.Contains(idType))
        {
            return new RegisterResult(null, "VALIDATION_FAILED",
                $"id_type must be one of: {string.Join(", ", validIdTypes)}.");
        }

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return new RegisterResult(null, "EMAIL_ALREADY_TAKEN", "An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName         = $"{firstName}.{lastName}",
            Email            = email,
            FirstName        = firstName,
            LastName         = lastName,
            IdNumber         = idNumber,
            IdType           = idType,
            Phone            = phone,
            PhoneExtra       = request.PhoneExtra?.Trim(),
            Location         = request.Location?.Trim(),
            Address          = request.Address?.Trim(),
            PhotoUrl         = request.PhotoUrl?.Trim(),
            EmailConfirmed   = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var firstError = createResult.Errors.FirstOrDefault();
            var code = firstError?.Code switch
            {
                "PasswordTooShort"              => "PASSWORD_WEAK",
                "PasswordRequiresDigit"         => "PASSWORD_WEAK",
                "PasswordRequiresUpper"         => "PASSWORD_WEAK",
                "PasswordRequiresLower"         => "PASSWORD_WEAK",
                "PasswordRequiresNonAlphanumeric" => "PASSWORD_WEAK",
                "DuplicateEmail"                => "EMAIL_ALREADY_TAKEN",
                "DuplicateUserName"             => "EMAIL_ALREADY_TAKEN",
                "InvalidEmail"                  => "VALIDATION_FAILED",
                _                               => "REGISTRATION_FAILED"
            };
            return new RegisterResult(null, code, firstError?.Description ?? "Unable to register user.");
        }

        var tokens = CreateTokens(user.Email ?? email, user.Id.ToString());
        return new RegisterResult(tokens, null, null);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return null;

        var passwordIsValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordIsValid) return null;

        return CreateTokens(user.Email ?? email, user.Id.ToString());
    }

    // ── Refresh / Logout ──────────────────────────────────────────────────────

    public Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken) ||
            !RefreshTokens.TryGetValue(request.RefreshToken, out var storedToken))
            return Task.FromResult<AuthResponse?>(null);

        if (storedToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            RefreshTokens.TryRemove(request.RefreshToken, out _);
            return Task.FromResult<AuthResponse?>(null);
        }

        RefreshTokens.TryRemove(request.RefreshToken, out _);
        return Task.FromResult<AuthResponse?>(CreateTokens(storedToken.Email, storedToken.UserId));
    }

    public Task<bool> LogoutAsync(LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Task.FromResult(false);

        return Task.FromResult(RefreshTokens.TryRemove(request.RefreshToken, out _));
    }

    // ── Recuperación de contraseña ────────────────────────────────────────────

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email)) return true; // no revelar

        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return true; // no revelar si el email existe

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // ── PUNTO DE INTEGRACIÓN DE EMAIL ────────────────────────────────────
        // Cuando se defina el proveedor de correo, personalizar el template y
        // la URL del deep-link mobile (scheme://reset-password?token=...&email=...)
        // antes de llamar a emailSender.SendAsync.
        // ─────────────────────────────────────────────────────────────────────
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);

        var resetLink = $"pawexplorers://reset-password?email={encodedEmail}&token={encodedToken}";

        var subject  = "Recupera tu contraseña — PawExplorers";
        var htmlBody =
            $"""
            <p>Hola {user.FirstName},</p>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
            <p><a href="{resetLink}">Restablecer contraseña</a></p>
            <p>Si no solicitaste este cambio, ignora este mensaje.</p>
            <p>El enlace expira en 1 hora.</p>
            """;

        await emailSender.SendAsync(email, subject, htmlBody);
        return true;
    }

    public async Task<(bool Success, string? ErrorCode)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email)       ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
            return (false, "VALIDATION_FAILED");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, "USER_NOT_FOUND");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var code = result.Errors.FirstOrDefault()?.Code switch
            {
                "InvalidToken"           => "INVALID_TOKEN",
                "PasswordTooShort"       => "PASSWORD_WEAK",
                "PasswordRequiresDigit"  => "PASSWORD_WEAK",
                "PasswordRequiresUpper"  => "PASSWORD_WEAK",
                "PasswordRequiresLower"  => "PASSWORD_WEAK",
                "PasswordRequiresNonAlphanumeric" => "PASSWORD_WEAK",
                _ => "RESET_FAILED"
            };
            return (false, code);
        }

        return (true, null);
    }

    // ── Cambio de contraseña (usuario autenticado) ────────────────────────────

    public async Task<(bool Success, string? ErrorCode)> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
            return (false, "VALIDATION_FAILED");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "USER_NOT_FOUND");

        var result = await userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var code = result.Errors.FirstOrDefault()?.Code switch
            {
                "PasswordMismatch"       => "WRONG_PASSWORD",
                "PasswordTooShort"       => "PASSWORD_WEAK",
                "PasswordRequiresDigit"  => "PASSWORD_WEAK",
                "PasswordRequiresUpper"  => "PASSWORD_WEAK",
                "PasswordRequiresLower"  => "PASSWORD_WEAK",
                "PasswordRequiresNonAlphanumeric" => "PASSWORD_WEAK",
                _ => "CHANGE_FAILED"
            };
            return (false, code);
        }

        return (true, null);
    }

    // ── Social login — Google ─────────────────────────────────────────────────

    public async Task<(AuthResponse? Tokens, string? ErrorCode)> GoogleLoginAsync(
        SocialLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return (null, "VALIDATION_FAILED");

        GoogleTokenPayload? payload;
        try
        {
            payload = await ValidateGoogleTokenAsync(request.IdToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Google token validation failed.");
            return (null, "INVALID_TOKEN");
        }

        if (payload is null) return (null, "INVALID_TOKEN");

        return await FindOrCreateSocialUserAsync(
            provider:      "Google",
            providerKey:   payload.Sub,
            email:         payload.Email,
            firstName:     payload.GivenName  ?? "Google",
            lastName:      payload.FamilyName ?? "User",
            photoUrl:      payload.Picture);
    }

    // ── Social login — Apple ──────────────────────────────────────────────────

    public async Task<(AuthResponse? Tokens, string? ErrorCode)> AppleLoginAsync(
        SocialLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return (null, "VALIDATION_FAILED");

        AppleTokenPayload? payload;
        try
        {
            payload = await ValidateAppleTokenAsync(request.IdToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Apple token validation failed.");
            return (null, "INVALID_TOKEN");
        }

        if (payload is null) return (null, "INVALID_TOKEN");

        // Apple solo entrega nombre/apellido en el primer login; los sucesivos no lo incluyen.
        return await FindOrCreateSocialUserAsync(
            provider:      "Apple",
            providerKey:   payload.Sub,
            email:         payload.Email,
            firstName:     payload.GivenName  ?? "Apple",
            lastName:      payload.FamilyName ?? "User",
            photoUrl:      null);
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    /// <summary>
    /// Busca usuario por LoginProvider+ProviderKey o por email.
    /// Si no existe, lo crea sin contraseña (social-only account).
    /// </summary>
    private async Task<(AuthResponse? Tokens, string? ErrorCode)> FindOrCreateSocialUserAsync(
        string provider, string providerKey, string? email,
        string firstName, string lastName, string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (null, "EMAIL_REQUIRED");

        // 1. Buscar por login externo ya vinculado
        var user = await userManager.FindByLoginAsync(provider, providerKey);

        // 2. Buscar por email (puede que el usuario exista con password)
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                // 3. Crear cuenta nueva sin contraseña
                user = new ApplicationUser
                {
                    UserName       = $"{firstName}.{lastName}",
                    Email          = email,
                    FirstName      = firstName,
                    LastName       = lastName,
                    IdNumber       = "SOCIAL",
                    IdType         = "SOCIAL",
                    Phone          = "0",
                    PhotoUrl       = photoUrl,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    logger.LogError("Could not create social user: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return (null, "REGISTRATION_FAILED");
                }
            }

            // Vincular el proveedor externo al usuario (nuevo o existente)
            await userManager.AddLoginAsync(user,
                new UserLoginInfo(provider, providerKey, provider));
        }

        var tokens = CreateTokens(user.Email ?? email, user.Id.ToString());
        return (tokens, null);
    }

    /// <summary>
    /// Valida el id_token de Google contra las claves públicas de Google.
    /// Documentación: https://developers.google.com/identity/sign-in/web/backend-auth
    /// </summary>
    private async Task<GoogleTokenPayload?> ValidateGoogleTokenAsync(string idToken)
    {
        var clientId = configuration["SocialAuth:Google:ClientId"]
            ?? throw new InvalidOperationException("SocialAuth:Google:ClientId not configured.");

        // Google publica sus claves en este endpoint; el handler las cachea automáticamente.
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuers             = ["accounts.google.com", "https://accounts.google.com"],
            ValidateAudience         = true,
            ValidAudience            = clientId,
            ValidateLifetime         = true,
            IssuerSigningKeyResolver = (_, _, _, _) =>
            {
                // Google usa RS256; las claves se obtienen del endpoint JWKS.
                var jwksUri = "https://www.googleapis.com/oauth2/v3/certs";
                var jwks    = new JsonWebKeySet(
                    new HttpClient().GetStringAsync(jwksUri).GetAwaiter().GetResult());
                return jwks.GetSigningKeys();
            }
        };

        var handler    = new JwtSecurityTokenHandler();
        var principal  = handler.ValidateToken(idToken, validationParameters, out _);
        var claims     = principal.Claims.ToDictionary(c => c.Type, c => c.Value);

        return await Task.FromResult(new GoogleTokenPayload(
            Sub:        claims.GetValueOrDefault(JwtRegisteredClaimNames.Sub)   ?? string.Empty,
            Email:      claims.GetValueOrDefault(JwtRegisteredClaimNames.Email) ?? string.Empty,
            GivenName:  claims.GetValueOrDefault("given_name"),
            FamilyName: claims.GetValueOrDefault("family_name"),
            Picture:    claims.GetValueOrDefault("picture")));
    }

    /// <summary>
    /// Valida el id_token de Apple contra las claves públicas de Apple.
    /// Documentación: https://developer.apple.com/documentation/sign_in_with_apple/sign_in_with_apple_rest_api/verifying_a_user
    /// </summary>
    private async Task<AppleTokenPayload?> ValidateAppleTokenAsync(string idToken)
    {
        var clientId = configuration["SocialAuth:Apple:ClientId"]
            ?? throw new InvalidOperationException("SocialAuth:Apple:ClientId not configured.");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = "https://appleid.apple.com",
            ValidateAudience         = true,
            ValidAudience            = clientId,
            ValidateLifetime         = true,
            IssuerSigningKeyResolver = (_, _, _, _) =>
            {
                var jwksUri = "https://appleid.apple.com/auth/keys";
                var jwks    = new JsonWebKeySet(
                    new HttpClient().GetStringAsync(jwksUri).GetAwaiter().GetResult());
                return jwks.GetSigningKeys();
            }
        };

        var handler   = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(idToken, validationParameters, out _);
        var claims    = principal.Claims.ToDictionary(c => c.Type, c => c.Value);

        // Apple solo incluye nombre/apellido en el payload del PRIMER login.
        // Los sucesivos deben obtenerse del body del request (el cliente mobile los envía).
        return await Task.FromResult(new AppleTokenPayload(
            Sub:        claims.GetValueOrDefault(JwtRegisteredClaimNames.Sub)   ?? string.Empty,
            Email:      claims.GetValueOrDefault(JwtRegisteredClaimNames.Email) ?? string.Empty,
            GivenName:  claims.GetValueOrDefault("given_name"),
            FamilyName: claims.GetValueOrDefault("family_name")));
    }

    private AuthResponse CreateTokens(string email, string userId)
    {
        var accessExpiresAt  = DateTimeOffset.UtcNow.AddHours(1);
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(7);

        var accessToken  = CreateAccessToken(email, userId, accessExpiresAt);
        var refreshToken = Guid.NewGuid().ToString("N");

        RefreshTokens[refreshToken] = new RefreshTokenRecord(email, userId, refreshExpiresAt);

        return new AuthResponse(accessToken, refreshToken, accessExpiresAt);
    }

    private string CreateAccessToken(string email, string userId, DateTimeOffset expiresAt)
    {
        var issuer   = configuration["Jwt:Issuer"]   ?? "balto";
        var audience = configuration["Jwt:Audience"] ?? "balto.api";
        var key      = configuration["Jwt:Key"]      ?? "dev-only-change-this-secret-key-32-chars";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier,     userId)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record RefreshTokenRecord(string Email, string UserId, DateTimeOffset ExpiresAt);

    private sealed record GoogleTokenPayload(
        string Sub, string Email,
        string? GivenName, string? FamilyName, string? Picture);

    private sealed record AppleTokenPayload(
        string Sub, string Email,
        string? GivenName, string? FamilyName);
}
