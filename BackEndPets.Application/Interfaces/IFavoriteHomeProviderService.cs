using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IFavoriteHomeProviderService
{
    Task<IReadOnlyCollection<FavoriteHomeProviderResponse>> GetMyFavoritesAsync(Guid userId);
    Task<(bool Success, string? ErrorCode)> AddFavoriteAsync(Guid userId, Guid providerId);
    Task<(bool Success, string? ErrorCode)> RemoveFavoriteAsync(Guid userId, Guid providerId);
}
