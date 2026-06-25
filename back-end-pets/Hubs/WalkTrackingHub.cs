using System.Security.Claims;
using BackEndPets.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BackEndPets.API.Hubs;

[Authorize]
public sealed class WalkTrackingHub(
    IWalkSessionRepository sessionRepo,
    IPetWalkingHistoryRepository historyRepo,
    IWalkerRepository walkerRepo) : Hub
{
    public async Task JoinWalkGroup(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            await Clients.Caller.SendAsync("Error", "Invalid session id.");
            return;
        }

        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized.");
            return;
        }

        var session = await sessionRepo.GetByIdAsync(sessionGuid);
        if (session is null)
        {
            await Clients.Caller.SendAsync("Error", "Session not found.");
            return;
        }

        var history = await historyRepo.GetByIdAsync(session.PetWalkingHistoryId);
        if (history is null)
        {
            await Clients.Caller.SendAsync("Error", "History not found.");
            return;
        }

        var walker = await walkerRepo.GetByUserIdAsync(userId);
        var isWalker = walker is not null && walker.Id == history.WalkerId;
        var isOwner = history.UserId == userId;

        if (!isWalker && !isOwner)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"walk-{sessionId}");
        await Clients.Caller.SendAsync("JoinedWalkGroup", sessionId);
    }

    public async Task LeaveWalkGroup(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"walk-{sessionId}");
    }
}
