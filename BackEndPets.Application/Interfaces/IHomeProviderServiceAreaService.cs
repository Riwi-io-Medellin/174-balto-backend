using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeProviderServiceAreaService
{
    Task<(IReadOnlyCollection<HomeProviderServiceAreaResponse>? Result, string? ErrorCode)> GetMyServiceAreasAsync(
        Guid userId);

    Task<(IReadOnlyCollection<HomeProviderServiceAreaResponse>? Result, string? ErrorCode)> ReplaceMyServiceAreasAsync(
        Guid userId, IEnumerable<HomeProviderServiceAreaRequest> areas);
}
