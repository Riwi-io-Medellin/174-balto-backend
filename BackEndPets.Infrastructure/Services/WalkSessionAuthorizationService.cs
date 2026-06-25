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

        var walker = await walkerRepo.GetByUserIdAsync(userId);
        var isWalker = walker is not null && walker.Id == session.WalkerId;
        if (isWalker)
            return new WalkGroupAccessResult(true, null);

        var histories = await historyRepo.GetBySessionIdAsync(sessionId);
        var isOwner = histories.Any(h => h.UserId == userId);

        return isOwner
            ? new WalkGroupAccessResult(true, null)
            : new WalkGroupAccessResult(false, "Unauthorized.");
    }
}
