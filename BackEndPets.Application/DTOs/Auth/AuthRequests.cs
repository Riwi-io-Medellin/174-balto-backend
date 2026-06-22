namespace BackEndPets.Application.DTOs.Auth;

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record LogoutRequest(
    string RefreshToken);

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string IdNumber,
    string IdType,
    string Phone,
    string? PhoneExtra = null,
    string? Location = null,
    string? Address = null,
    string? PhotoUrl = null);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);

public sealed record RegisterResult(
    AuthResponse? Tokens,
    string? ErrorCode,
    string? ErrorMessage);
