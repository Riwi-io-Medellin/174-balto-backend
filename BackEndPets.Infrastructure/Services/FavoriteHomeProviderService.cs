using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class FavoriteHomeProviderService(
    IFavoriteHomeProviderRepository favoriteRepository,
    IHomeServiceProviderRepository providerRepository) : IFavoriteHomeProviderService
{
    public async Task<IReadOnlyCollection<FavoriteHomeProviderResponse>> GetMyFavoritesAsync(Guid userId)
    {
        var favorites = await favoriteRepository.GetByUserIdAsync(userId);
        var result = new List<FavoriteHomeProviderResponse>(favorites.Count);
        foreach (var favorite in favorites)
        {
            var projection = await providerRepository.GetByIdWithUserAsync(favorite.ProviderId);
            if (projection is not null)
                result.Add(new FavoriteHomeProviderResponse(
                    favorite.ProviderId,
                    $"{projection.FirstName} {projection.LastName}",
                    projection.PhotoUrl,
                    favorite.CreatedAt));
        }
        return result;
    }

    public async Task<(bool Success, string? ErrorCode)> AddFavoriteAsync(Guid userId, Guid providerId)
    {
        var provider = await providerRepository.GetByIdAsync(providerId);
        if (provider is null) return (false, "PROVIDER_NOT_FOUND");

        if (await favoriteRepository.ExistsAsync(userId, providerId))
            return (false, "ALREADY_FAVORITED");

        await favoriteRepository.AddAsync(new FavoriteHomeProvider { UserId = userId, ProviderId = providerId });
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorCode)> RemoveFavoriteAsync(Guid userId, Guid providerId)
    {
        var removed = await favoriteRepository.RemoveAsync(userId, providerId);
        return removed ? (true, null) : (false, "NOT_FAVORITED");
    }
}
