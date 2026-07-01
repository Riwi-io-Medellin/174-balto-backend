namespace BackEndPets.Application.DTOs.HomeServices;

public sealed record FavoriteHomeProviderResponse(
    Guid ProviderId,
    string FullName,
    string? ProfilePhoto,
    DateTime CreatedAt);
