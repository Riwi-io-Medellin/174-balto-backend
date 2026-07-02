using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeServiceMarketplaceService
{
    Task<(PagedResult<HomeServiceProviderSummaryResponse>? Result, string? ErrorCode)> SearchAsync(
        HomeServiceSearchQuery query);

    Task<(HomeServiceProviderDetailResponse? Result, string? ErrorCode)> GetDetailAsync(
        Guid providerId, DateOnly? date, int? durationMinutes);
}
