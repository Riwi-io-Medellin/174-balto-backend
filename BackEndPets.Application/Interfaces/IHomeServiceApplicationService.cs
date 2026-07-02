using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeServiceApplicationService
{
    Task<(HomeServiceApplyResponse? Result, string? ErrorCode)> ApplyAsync(
        Guid userId,
        Stream documentStream,
        string documentFileName,
        HomeServiceApplyRequest request);
}
