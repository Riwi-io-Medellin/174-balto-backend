using System.Text.Json;
using BackEndPets.Application.DTOs.Admin;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Services;

public sealed class AdminOperationsService(AppIdentityDbContext dbContext) : IAdminOperationsService
{
    private static readonly HashSet<string> EntityActions = ["suspend", "reactivate", "delete"];
    private static readonly HashSet<string> AlertStatuses = ["under_review", "resolved", "dismissed", "false_report"];

    public async Task<IReadOnlyCollection<AdminUserListItemResponse>> GetUsersAsync(
        string? search,
        string? status,
        string? role,
        DateTime? from,
        DateTime? to)
    {
        var query = dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Email!.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = Normalize(status);
            query = query.Where(u => u.AdminStatus == normalizedStatus);
        }

        if (from.HasValue) query = query.Where(u => u.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(u => u.CreatedAt <= to.Value);

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();
        var walkerUserIds = await dbContext.Walkers
            .AsNoTracking()
            .Where(w => userIds.Contains(w.UserId))
            .Select(w => w.UserId)
            .ToListAsync();
        var walkerSet = walkerUserIds.ToHashSet();

        var items = users.Select(u =>
        {
            var itemRole = walkerSet.Contains(u.Id) ? "walker" : "user";
            return ToUserItem(u, itemRole);
        });

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = Normalize(role);
            items = items.Where(u => u.Role == normalizedRole);
        }

        return items.ToList();
    }

    public async Task<IReadOnlyCollection<AdminWalkerListItemResponse>> GetWalkersAsync(
        string? search,
        string? status,
        string? verificationStatus,
        DateTime? from,
        DateTime? to)
    {
        var query = dbContext.Walkers
            .AsNoTracking()
            .Join(dbContext.Users.AsNoTracking(),
                w => w.UserId,
                u => u.Id,
                (w, u) => new { Walker = w, User = u });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.User.FirstName.ToLower().Contains(term) ||
                x.User.LastName.ToLower().Contains(term) ||
                x.User.Email!.ToLower().Contains(term) ||
                (x.Walker.WorkLocation != null && x.Walker.WorkLocation.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = Normalize(status);
            query = query.Where(x => x.Walker.AdminStatus == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(verificationStatus))
        {
            var normalizedVerification = Normalize(verificationStatus);
            query = query.Where(x => x.Walker.VerificationStatus == normalizedVerification);
        }

        if (from.HasValue) query = query.Where(x => x.Walker.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.Walker.CreatedAt <= to.Value);

        var walkers = await query
            .OrderByDescending(x => x.Walker.CreatedAt)
            .ToListAsync();

        return walkers
            .Select(x => ToWalkerItem(x.Walker, x.User))
            .ToList();
    }

    public async Task<IReadOnlyCollection<AdminCommunityAlertResponse>> GetCommunityAlertsAsync(
        string? search,
        string? status,
        string? alertType,
        DateTime? from,
        DateTime? to)
    {
        var query = dbContext.CommunityAlerts
            .AsNoTracking()
            .Join(dbContext.Users.AsNoTracking(),
                a => a.ReporterUserId,
                u => u.Id,
                (a, u) => new { Alert = a, Reporter = u });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Alert.PetName.ToLower().Contains(term) ||
                x.Alert.Description.ToLower().Contains(term) ||
                (x.Alert.LastSeenLocation != null && x.Alert.LastSeenLocation.ToLower().Contains(term)) ||
                x.Reporter.FirstName.ToLower().Contains(term) ||
                x.Reporter.LastName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = Normalize(status);
            query = query.Where(x => x.Alert.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(alertType))
        {
            var normalizedType = Normalize(alertType);
            query = query.Where(x => x.Alert.AlertType == normalizedType);
        }

        if (from.HasValue) query = query.Where(x => x.Alert.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.Alert.CreatedAt <= to.Value);

        var alerts = await query
            .OrderByDescending(x => x.Alert.CreatedAt)
            .ToListAsync();

        return alerts
            .Select(x => ToAlertItem(x.Alert, x.Reporter))
            .ToList();
    }

    public async Task<AdminOperationResult<AdminUserListItemResponse>> UpdateUserAsync(
        Guid actorUserId,
        Guid userId,
        AdminEntityActionRequest request)
    {
        var action = Normalize(request.Action);
        var reason = request.Reason.Trim();
        if (!EntityActions.Contains(action)) return AdminOperationResult<AdminUserListItemResponse>.Fail("INVALID_ACTION");
        if (string.IsNullOrWhiteSpace(reason)) return AdminOperationResult<AdminUserListItemResponse>.Fail("REASON_REQUIRED");
        if (RequiresConfirmation(action) && !request.ConfirmImpact) return AdminOperationResult<AdminUserListItemResponse>.Fail("CONFIRMATION_REQUIRED");

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return AdminOperationResult<AdminUserListItemResponse>.Fail("USER_NOT_FOUND");

        var previous = new { user.AdminStatus, user.AdminReason, user.DeletedAt };
        ApplyUserAction(user, action, actorUserId, reason);
        await WriteAuditAsync(actorUserId, $"user.{action}", "user", user.Id, reason, previous);
        await dbContext.SaveChangesAsync();

        var isWalker = await dbContext.Walkers.AnyAsync(w => w.UserId == user.Id);
        return AdminOperationResult<AdminUserListItemResponse>.Ok(ToUserItem(user, isWalker ? "walker" : "user"));
    }

    public async Task<AdminOperationResult<AdminWalkerListItemResponse>> UpdateWalkerAsync(
        Guid actorUserId,
        Guid walkerId,
        AdminEntityActionRequest request)
    {
        var action = Normalize(request.Action);
        var reason = request.Reason.Trim();
        if (!EntityActions.Contains(action)) return AdminOperationResult<AdminWalkerListItemResponse>.Fail("INVALID_ACTION");
        if (string.IsNullOrWhiteSpace(reason)) return AdminOperationResult<AdminWalkerListItemResponse>.Fail("REASON_REQUIRED");
        if (RequiresConfirmation(action) && !request.ConfirmImpact) return AdminOperationResult<AdminWalkerListItemResponse>.Fail("CONFIRMATION_REQUIRED");

        var walker = await dbContext.Walkers.FirstOrDefaultAsync(w => w.Id == walkerId);
        if (walker is null) return AdminOperationResult<AdminWalkerListItemResponse>.Fail("WALKER_NOT_FOUND");

        var user = await dbContext.Users.FirstAsync(u => u.Id == walker.UserId);
        var previous = new { walker.AdminStatus, walker.AdminReason, walker.DeletedAt, walker.IsAcceptingBookings };
        ApplyWalkerAction(walker, action, actorUserId, reason);
        await WriteAuditAsync(actorUserId, $"walker.{action}", "walker", walker.Id, reason, previous);
        await dbContext.SaveChangesAsync();

        return AdminOperationResult<AdminWalkerListItemResponse>.Ok(ToWalkerItem(walker, user));
    }

    public async Task<AdminOperationResult<AdminCommunityAlertResponse>> ModerateAlertAsync(
        Guid actorUserId,
        Guid alertId,
        AdminAlertModerationRequest request)
    {
        var status = Normalize(request.Status);
        var reason = request.Reason.Trim();
        if (!AlertStatuses.Contains(status)) return AdminOperationResult<AdminCommunityAlertResponse>.Fail("INVALID_ALERT_STATUS");
        if (string.IsNullOrWhiteSpace(reason)) return AdminOperationResult<AdminCommunityAlertResponse>.Fail("REASON_REQUIRED");
        if (!request.ConfirmImpact) return AdminOperationResult<AdminCommunityAlertResponse>.Fail("CONFIRMATION_REQUIRED");

        var alert = await dbContext.CommunityAlerts.FirstOrDefaultAsync(a => a.Id == alertId);
        if (alert is null) return AdminOperationResult<AdminCommunityAlertResponse>.Fail("ALERT_NOT_FOUND");
        if (alert.UpdatedAt != request.ExpectedUpdatedAt) return AdminOperationResult<AdminCommunityAlertResponse>.Fail("ALERT_CONFLICT");

        var reporter = await dbContext.Users.FirstAsync(u => u.Id == alert.ReporterUserId);
        var previous = new
        {
            alert.Status,
            alert.ModerationReason,
            alert.EvidenceUrl,
            alert.UpdatedAt
        };
        alert.Status = status;
        alert.ModerationReason = reason;
        alert.ModeratedByUserId = actorUserId;
        alert.ModeratedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        await WriteAuditAsync(actorUserId, $"alert.moderate.{status}", "community_alert", alert.Id, reason, previous);
        await dbContext.SaveChangesAsync();

        return AdminOperationResult<AdminCommunityAlertResponse>.Ok(ToAlertItem(alert, reporter));
    }

    public async Task<IReadOnlyCollection<AdminAuditLogResponse>> GetAuditLogsAsync(
        string? entityType,
        Guid? entityId,
        int take)
    {
        var query = dbContext.AdminAuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(l => l.EntityType == Normalize(entityType));
        }

        if (entityId.HasValue)
        {
            query = query.Where(l => l.EntityId == entityId.Value);
        }

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .Select(l => new AdminAuditLogResponse(
                l.Id,
                l.ActorUserId,
                l.Action,
                l.EntityType,
                l.EntityId,
                l.Reason,
                l.CreatedAt))
            .ToListAsync();
    }

    private static AdminUserListItemResponse ToUserItem(ApplicationUser user, string role) =>
        new(
            user.Id,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email ?? string.Empty,
            user.Phone,
            user.Location,
            role,
            user.AdminStatus,
            user.AdminStatus is "suspended" or "deleted" ? "high" : "low",
            user.CreatedAt);

    private static AdminWalkerListItemResponse ToWalkerItem(Walker walker, ApplicationUser user)
    {
        var risk = walker.AdminStatus is "suspended" or "deleted"
            ? "high"
            : walker.VerificationStatus == "pending"
                ? "medium"
                : "low";

        return new(
            walker.Id,
            walker.UserId,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email ?? string.Empty,
            walker.WorkLocation,
            walker.VerificationStatus,
            walker.AdminStatus,
            risk,
            walker.IsAcceptingBookings,
            walker.CreatedAt,
            walker.UpdatedAt);
    }

    private static AdminCommunityAlertResponse ToAlertItem(CommunityAlert alert, ApplicationUser reporter)
    {
        var risk = alert.Status is "false_report" or "dismissed"
            ? "high"
            : alert.EvidenceUrl is null
                ? "medium"
                : "low";

        return new(
            alert.Id,
            alert.ReporterUserId,
            $"{reporter.FirstName} {reporter.LastName}".Trim(),
            alert.AlertType,
            alert.PetName,
            alert.Species,
            alert.Description,
            alert.LastSeenLocation,
            alert.EvidenceUrl,
            alert.Status,
            risk,
            alert.ModerationReason,
            alert.CreatedAt,
            alert.UpdatedAt);
    }

    private static void ApplyUserAction(ApplicationUser user, string action, Guid actorUserId, string reason)
    {
        user.AdminStatus = action == "reactivate" ? "active" : action == "delete" ? "deleted" : "suspended";
        user.AdminReason = reason;
        user.AdminModeratedByUserId = actorUserId;
        user.AdminModeratedAt = DateTime.UtcNow;
        user.DeletedAt = action == "delete" ? DateTime.UtcNow : null;
    }

    private static void ApplyWalkerAction(Walker walker, string action, Guid actorUserId, string reason)
    {
        walker.AdminStatus = action == "reactivate" ? "active" : action == "delete" ? "deleted" : "suspended";
        walker.AdminReason = reason;
        walker.AdminModeratedByUserId = actorUserId;
        walker.AdminModeratedAt = DateTime.UtcNow;
        walker.DeletedAt = action == "delete" ? DateTime.UtcNow : null;
        walker.UpdatedAt = DateTime.UtcNow;

        if (action is "suspend" or "delete")
        {
            walker.IsAcceptingBookings = false;
        }
    }

    private async Task WriteAuditAsync(
        Guid actorUserId,
        string action,
        string entityType,
        Guid entityId,
        string reason,
        object snapshot)
    {
        await dbContext.AdminAuditLogs.AddAsync(new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Reason = reason,
            SnapshotJson = JsonSerializer.Serialize(snapshot),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static bool RequiresConfirmation(string action) => action is "suspend" or "delete";

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();
}
