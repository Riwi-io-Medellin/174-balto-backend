namespace BackEndPets.Application.DTOs.Users;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Role,
    bool IsActive = true);

public sealed record UpdateUserRequest(
    string FullName,
    string Email,
    string Role,
    bool IsActive);

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAt);
