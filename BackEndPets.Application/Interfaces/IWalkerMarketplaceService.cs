using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Walkers;

namespace BackEndPets.Application.Interfaces;

public interface IWalkerMarketplaceService
{
    Task<(PagedResult<WalkerSummaryResponse>? Result, string? ErrorCode)> SearchAsync(WalkerSearchQuery query);

    Task<(WalkerDetailResponse? Result, string? ErrorCode)> GetDetailAsync(
        Guid walkerId, DateOnly? date, int? durationMinutes);
}
