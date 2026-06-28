namespace BackEndPets.Domain.Entities;

public sealed record WalkerUserProjection(
    Walker Walker,
    string FirstName,
    string LastName,
    string? PhotoUrl);
