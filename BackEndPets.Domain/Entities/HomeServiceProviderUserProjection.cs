namespace BackEndPets.Domain.Entities;

public sealed record HomeServiceProviderUserProjection(
    HomeServiceProvider Provider,
    string FirstName,
    string LastName,
    string? PhotoUrl);
