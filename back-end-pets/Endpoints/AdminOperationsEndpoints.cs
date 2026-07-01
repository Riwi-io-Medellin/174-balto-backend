using System.Security.Claims;
using BackEndPets.Application.DTOs.Admin;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class AdminOperationsEndpoints
{
    public static IEndpointRouteBuilder MapAdminOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/operations")
            .WithTags("Admin Operations")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/users", async (
                string? search,
                string? status,
                string? role,
                DateTime? from,
                DateTime? to,
                IAdminOperationsService service) =>
            Results.Ok(await service.GetUsersAsync(search, status, role, from, to)))
            .WithName("GetAdminOperationUsers")
            .WithSummary("List and filter users for support operations")
            .Produces<IReadOnlyCollection<AdminUserListItemResponse>>(StatusCodes.Status200OK);

        group.MapPatch("/users/{userId:guid}/action", async (
            Guid userId,
            AdminEntityActionRequest request,
            ClaimsPrincipal principal,
            IAdminOperationsService service) =>
        {
            var result = await service.UpdateUserAsync(GetActorUserId(principal), userId, request);
            return ToActionResult(result, "User action could not be completed.");
        })
        .WithName("UpdateAdminOperationUser")
        .WithSummary("Suspend, reactivate, or logically delete a user")
        .Produces<AdminUserListItemResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/walkers", async (
                string? search,
                string? status,
                string? verificationStatus,
                DateTime? from,
                DateTime? to,
                IAdminOperationsService service) =>
            Results.Ok(await service.GetWalkersAsync(search, status, verificationStatus, from, to)))
            .WithName("GetAdminOperationWalkers")
            .WithSummary("List and filter walkers for support operations")
            .Produces<IReadOnlyCollection<AdminWalkerListItemResponse>>(StatusCodes.Status200OK);

        group.MapPatch("/walkers/{walkerId:guid}/action", async (
            Guid walkerId,
            AdminEntityActionRequest request,
            ClaimsPrincipal principal,
            IAdminOperationsService service) =>
        {
            var result = await service.UpdateWalkerAsync(GetActorUserId(principal), walkerId, request);
            return ToActionResult(result, "Walker action could not be completed.");
        })
        .WithName("UpdateAdminOperationWalker")
        .WithSummary("Suspend, reactivate, or logically delete a walker")
        .Produces<AdminWalkerListItemResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/alerts", async (
                string? search,
                string? status,
                string? alertType,
                DateTime? from,
                DateTime? to,
                IAdminOperationsService service) =>
            Results.Ok(await service.GetCommunityAlertsAsync(search, status, alertType, from, to)))
            .WithName("GetAdminOperationAlerts")
            .WithSummary("List and filter lost/found community alerts")
            .Produces<IReadOnlyCollection<AdminCommunityAlertResponse>>(StatusCodes.Status200OK);

        group.MapPatch("/alerts/{alertId:guid}/moderation", async (
            Guid alertId,
            AdminAlertModerationRequest request,
            ClaimsPrincipal principal,
            IAdminOperationsService service) =>
        {
            var result = await service.ModerateAlertAsync(GetActorUserId(principal), alertId, request);
            return ToActionResult(result, "Alert moderation could not be completed.");
        })
        .WithName("ModerateAdminOperationAlert")
        .WithSummary("Moderate a lost/found community alert with concurrency protection")
        .Produces<AdminCommunityAlertResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/audit", async (
                string? entityType,
                Guid? entityId,
                int? take,
                IAdminOperationsService service) =>
            Results.Ok(await service.GetAuditLogsAsync(entityType, entityId, take ?? 50)))
            .WithName("GetAdminOperationAudit")
            .WithSummary("List admin audit trail")
            .Produces<IReadOnlyCollection<AdminAuditLogResponse>>(StatusCodes.Status200OK);

        return app;
    }

    private static Guid GetActorUserId(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed) ? parsed : Guid.Empty;
    }

    private static IResult ToActionResult<T>(AdminOperationResult<T> result, string message)
    {
        if (result.Success && result.Data is not null)
        {
            return Results.Ok(result.Data);
        }

        var code = result.ErrorCode ?? "ADMIN_ACTION_FAILED";
        return code switch
        {
            "USER_NOT_FOUND" or "WALKER_NOT_FOUND" or "ALERT_NOT_FOUND" =>
                Results.NotFound(new ApiErrorResponse(message, code)),
            "ALERT_CONFLICT" =>
                Results.Conflict(new ApiErrorResponse(
                    "Alert state changed before moderation. Refresh and review the latest evidence.",
                    code)),
            _ => Results.BadRequest(new ApiErrorResponse(message, code))
        };
    }
}
