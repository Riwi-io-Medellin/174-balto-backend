using BackEndPets.Application.DTOs.WalkingHistory;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkingHistoryService(
    IPetWalkingHistoryRepository historyRepository,
    IPetRepository petRepository,
    IWalkerRepository walkerRepository) : IWalkingHistoryService
{
    public async Task<(WalkingHistoryResponse? History, string? ErrorCode)> CreateAsync(
        Guid userId, CreateWalkingHistoryRequest request)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != userId) return (null, "PET_NOT_OWNED");

        var walker = await walkerRepository.GetByIdAsync(request.WalkerId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var history = new PetWalkingHistory
        {
            UserId = userId,
            PetId = request.PetId,
            WalkerId = request.WalkerId,
            StartTime = request.StartTime,
            Cost = request.Cost
        };

        var created = await historyRepository.CreateAsync(history);
        return (MapResponse(created), null);
    }

    public async Task<WalkingHistoryResponse?> GetByIdAsync(Guid id)
    {
        var history = await historyRepository.GetByIdAsync(id);
        return history is null ? null : MapResponse(history);
    }

    public async Task<IReadOnlyCollection<WalkingHistoryResponse>> GetMyHistoryAsync(Guid userId) =>
        (await historyRepository.GetByUserIdAsync(userId))
            .Select(MapResponse)
            .ToList();

    public async Task<IReadOnlyCollection<WalkingHistoryResponse>> GetByWalkerAsync(Guid userId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return [];

        return (await historyRepository.GetByWalkerIdAsync(walker.Id))
            .Select(MapResponse)
            .ToList();
    }

    private static WalkingHistoryResponse MapResponse(PetWalkingHistory h) =>
        new(h.Id, h.UserId, h.PetId, h.WalkerId, h.Cost, h.StartTime, h.EndTime, h.CreatedAt);
}