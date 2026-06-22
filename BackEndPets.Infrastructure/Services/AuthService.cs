using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackEndPets.Application.DTOs.Auth;
using BackEndPets.Application.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BackEndPets.Infrastructure.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IAuthService
{
    private static readonly ConcurrentDictionary<string, RefreshTokenRecord> RefreshTokens = new();

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var idNumber = request.IdNumber?.Trim() ?? string.Empty;
        var idType = request.IdType?.Trim() ?? string.Empty;
        var phone = request.Phone?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(idNumber) ||
            string.IsNullOrWhiteSpace(idType) ||
            string.IsNullOrWhiteSpace(phone))
        {
            return new RegisterResult(null, "VALIDATION_FAILED", "All fields are required.");
        }

        string[] validIdTypes = ["CC", "CE", "Passport", "TI"];
        if (!validIdTypes.Contains(idType))
        {
            return new RegisterResult(null, "VALIDATION_FAILED", $"id_type must be one of: {string.Join(", ", validIdTypes)}.");
        }

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return new RegisterResult(null, "EMAIL_ALREADY_TAKEN", "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IdNumber = idNumber,
            IdType = idType,
            Phone = phone,
            PhoneExtra = request.PhoneExtra?.Trim(),
            Location = request.Location?.Trim(),
            Address = request.Address?.Trim(),
            PhotoUrl = request.PhotoUrl?.Trim(),
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var firstError = createResult.Errors.FirstOrDefault();
            var code = firstError?.Code switch
            {
                "PasswordTooShort" => "PASSWORD_WEAK",
                "PasswordRequiresDigit" => "PASSWORD_WEAK",
                "PasswordRequiresUpper" => "PASSWORD_WEAK",
                "PasswordRequiresLower" => "PASSWORD_WEAK",
                "PasswordRequiresNonAlphanumeric" => "PASSWORD_WEAK",
                "DuplicateEmail" => "EMAIL_ALREADY_TAKEN",
                "DuplicateUserName" => "EMAIL_ALREADY_TAKEN",
                "InvalidEmail" => "VALIDATION_FAILED",
                _ => "REGISTRATION_FAILED"
            };
            return new RegisterResult(null, code, firstError?.Description ?? "Unable to register user.");
        }

        var tokens = CreateTokens(user.Email ?? email, user.Id.ToString());
        return new RegisterResult(tokens, null, null);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var passwordIsValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordIsValid)
        {
            return null;
        }

        return CreateTokens(user.Email ?? email, user.Id.ToString());
    }

    public Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken) ||
            !RefreshTokens.TryGetValue(request.RefreshToken, out var storedToken))
        {
            return Task.FromResult<AuthResponse?>(null);
        }

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
        {
            return Task.FromResult(false);
        }

        var removed = RefreshTokens.TryRemove(request.RefreshToken, out _);
        return Task.FromResult(removed);
    }

    private AuthResponse CreateTokens(string email, string userId)
    {
        var accessExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(7);

        var accessToken = CreateAccessToken(email, userId, accessExpiresAt);
        var refreshToken = Guid.NewGuid().ToString("N");

        RefreshTokens[refreshToken] = new RefreshTokenRecord(email, userId, refreshExpiresAt);

        return new AuthResponse(accessToken, refreshToken, accessExpiresAt);
    }

    private string CreateAccessToken(string email, string userId, DateTimeOffset expiresAt)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "balto";
        var audience = configuration["Jwt:Audience"] ?? "balto.api";
        var key = configuration["Jwt:Key"] ?? "dev-only-change-this-secret-key-32-chars";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId)
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
}