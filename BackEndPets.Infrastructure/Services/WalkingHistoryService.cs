using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.WalkingHistory;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkingHistoryService(
    IPetWalkingHistoryRepository historyRepository,
    IPetRepository petRepository,
    IWalkerRepository walkerRepository,
    IWalkBookingRepository walkBookingRepository,
    IWalkSessionRepository walkSessionRepository) : IWalkingHistoryService
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

    public async Task<PagedResult<WalkingHistoryResponse>> GetMyHistoryAsync(
        Guid userId, int page = 1, int pageSize = 20)
    {
        var stored = await historyRepository.GetByUserIdAsync(userId);
        var storedSessionIds = stored
            .Where(h => h.WalkSessionId.HasValue)
            .Select(h => h.WalkSessionId!.Value)
            .ToHashSet();

        // Legacy completed bookings never got a pet_walking_history row.
        // Read them straight from walk_bookings/walk_sessions instead of writing a backfill.
        var completedBookings = await walkBookingRepository.GetByClientUserIdAsync(userId, "completed");
        var orphanBookings = completedBookings
            .Where(b => b.WalkSessionId.HasValue && !storedSessionIds.Contains(b.WalkSessionId.Value))
            .ToList();

        var orphanSessions = await walkSessionRepository.GetByIdsAsync(
            orphanBookings.Select(b => b.WalkSessionId!.Value));

        var merged = stored
            .Select(MapResponse)
            .Concat(orphanBookings
                .Where(b => orphanSessions.ContainsKey(b.WalkSessionId!.Value))
                .Select(b => MapFromBooking(b, orphanSessions[b.WalkSessionId!.Value])))
            .OrderByDescending(h => h.StartTime)
            .ToList();

        var paged = merged.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<WalkingHistoryResponse>(paged, page, pageSize, merged.Count);
    }

    public async Task<PagedResult<WalkingHistoryResponse>> GetByWalkerAsync(
        Guid userId, int page = 1, int pageSize = 20)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null) return new PagedResult<WalkingHistoryResponse>([], page, pageSize, 0);

        var all = await historyRepository.GetByWalkerIdAsync(walker.Id);
        var totalCount = all.Count;
        var paged = all
            .OrderByDescending(h => h.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapResponse)
            .ToList();
        return new PagedResult<WalkingHistoryResponse>(paged, page, pageSize, totalCount);
    }

    private static WalkingHistoryResponse MapResponse(PetWalkingHistory h) =>
        new(h.Id, h.UserId, h.PetId, h.WalkerId, h.WalkSessionId, h.Cost, h.StartTime, h.EndTime, h.CreatedAt);

    // Synthetic row for a completed booking that has no pet_walking_history record.
    // Id reuses the booking's id since there's no history row to point to.
    private static WalkingHistoryResponse MapFromBooking(WalkBooking b, WalkSession s) =>
        new(b.Id, b.ClientUserId, b.PetId, b.WalkerId, b.WalkSessionId, b.TotalPrice, s.StartedAt, s.EndedAt, b.CreatedAt);
}