using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkSessionAuthorizationService(
    IWalkSessionRepository sessionRepo,
    IPetWalkingHistoryRepository historyRepo,
    IWalkerRepository walkerRepo) : IWalkSessionAuthorizationService
{
    public async Task<WalkGroupAccessResult> CanJoinWalkGroupAsync(Guid userId, Guid sessionId)
    {
        var session = await sessionRepo.GetByIdAsync(sessionId);
        if (session is null)
            return new WalkGroupAccessResult(false, "Session not found.");

        var history = await historyRepo.GetByIdAsync(session.PetWalkingHistoryId);
        if (history is null)
            return new WalkGroupAccessResult(false, "History not found.");

        var walker = await walkerRepo.GetByUserIdAsync(userId);
        var isWalker = walker is not null && walker.Id == history.WalkerId;
        var isOwner = history.UserId == userId;

        if (!isWalker && !isOwner)
            return new WalkGroupAccessResult(false, "Unauthorized.");

        return new WalkGroupAccessResult(true, null);
    }
}
