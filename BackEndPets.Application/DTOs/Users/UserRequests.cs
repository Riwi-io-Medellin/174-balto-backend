namespace BackEndPets.Application.DTOs.Users;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string IdNumber,
    string IdType,
    string? Location,
    string? Address,
    long Phone,
    long? PhoneExtra,
    string? PhotoUrl);

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string IdNumber,
    string IdType,
    string? Location,
    string? Address,
    long Phone,
    long? PhoneExtra,
    string? PhotoUrl);

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string IdNumber,
    string IdType,
    string? Location,
    string? Address,
    long Phone,
    long? PhoneExtra,
    string? PhotoUrl,
    DateTime CreatedAt);
