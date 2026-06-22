namespace BackEndPets.Application.DTOs.Users;

public sealed record CreateUserRequest(
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

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string IdNumber,
    string IdType,
    string Phone,
    string? PhoneExtra = null,
    string? Location = null,
    string? Address = null,
    string? PhotoUrl = null);

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string IdNumber,
    string IdType,
    string Phone,
    string? PhoneExtra,
    string? Location,
    string? Address,
    string? PhotoUrl,
    DateTime CreatedAt);