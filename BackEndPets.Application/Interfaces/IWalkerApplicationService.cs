using BackEndPets.Application.DTOs.Walkers;

namespace BackEndPets.Application.Interfaces;

public interface IWalkerApplicationService
{
    Task<(WalkerApplyResponse? Result, string? ErrorCode)> ApplyAsync(
        Guid userId,
        Stream documentStream,
        string documentFileName,
        WalkerApplyRequest request);
}
