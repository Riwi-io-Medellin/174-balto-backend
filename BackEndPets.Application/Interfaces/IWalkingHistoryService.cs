using BackEndPets.Application.DTOs.WalkingHistory;

namespace BackEndPets.Application.Interfaces;

public interface IWalkingHistoryService
{
    Task<(WalkingHistoryResponse? History, string? ErrorCode)> CreateAsync(
        Guid userId, CreateWalkingHistoryRequest request);
    Task<WalkingHistoryResponse?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<WalkingHistoryResponse>> GetMyHistoryAsync(Guid userId);
    Task<IReadOnlyCollection<WalkingHistoryResponse>> GetByWalkerAsync(Guid userId);
}