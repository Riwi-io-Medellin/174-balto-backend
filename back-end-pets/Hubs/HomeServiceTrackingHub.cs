using System.Security.Claims;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BackEndPets.API.Hubs;

[Authorize]
public sealed class HomeServiceTrackingHub(IHomeServiceSessionAuthorizationService authorizationService) : Hub
{
    public async Task JoinSessionGroup(string sessionId)
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

        var result = await authorizationService.CanJoinSessionGroupAsync(userId, sessionGuid);
        if (!result.CanAccess)
        {
            await Clients.Caller.SendAsync("Error", result.ErrorMessage);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"home-service-{sessionId}");
        await Clients.Caller.SendAsync("JoinedSessionGroup", sessionId);
    }

    public async Task LeaveSessionGroup(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"home-service-{sessionId}");
    }
}
