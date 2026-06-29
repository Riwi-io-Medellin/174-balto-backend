using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.WalkingHistory;

namespace BackEndPets.Application.Interfaces;

public interface IWalkingHistoryService
{
    Task<(WalkingHistoryResponse? History, string? ErrorCode)> CreateAsync(
        Guid userId, CreateWalkingHistoryRequest request);
    Task<WalkingHistoryResponse?> GetByIdAsync(Guid id);
    Task<PagedResult<WalkingHistoryResponse>> GetMyHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<PagedResult<WalkingHistoryResponse>> GetByWalkerAsync(Guid userId, int page = 1, int pageSize = 20);
}
