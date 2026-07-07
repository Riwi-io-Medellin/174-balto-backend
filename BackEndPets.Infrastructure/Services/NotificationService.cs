using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    IDeviceTokenRepository deviceTokenRepository,
    IPushNotificationSender pushSender,
    ILogger<NotificationService> logger) : INotificationService
{
    private static readonly string[] ValidTypes =
    [
        "walk_started", "walk_finished", "walk_cancelled", "walk_media_uploaded", "chat_message",
        "walker_assigned", "walker_approved", "walker_rejected",
        "lost_pet", "pet_tag_scanned", "pet_location_shared", "business_approved", "business_rejected", "system"
    ];

    private static readonly string[] ValidEntityTypes =
    [
        "walk_session", "pet", "walker", "business", "user"
    ];

    public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Body = request.Body,
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            Metadata = request.Metadata
        };

        var created = await notificationRepository.CreateAsync(notification);
        await SendPushAsync(created);
        return MapResponse(created);
    }

    private async Task SendPushAsync(Notification notification)
    {
        if (!pushSender.IsConfigured) return;

        try
        {
            var tokens = await deviceTokenRepository.GetTokensByUserIdAsync(notification.UserId);
            if (tokens.Count == 0) return;

            var invalidTokens = await pushSender.SendAsync(
                tokens,
                notification.Title,
                notification.Body,
                new Dictionary<string, string>
                {
                    ["type"] = notification.Type,
                    ["entityId"] = notification.EntityId?.ToString() ?? "",
                    ["entityType"] = notification.EntityType ?? "",
                },
                CancellationToken.None);

            if (invalidTokens.Count > 0)
            {
                await deviceTokenRepository.RemoveTokensAsync(invalidTokens);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to send push notification for {Type}: {ExceptionType}",
                notification.Type, ex.GetType().Name);
        }
    }

    public async Task<NotificationSummaryResponse> GetMyNotificationsAsync(
        Guid userId, bool? unreadOnly = null, int page = 1, int pageSize = 20)
    {
        var all = await notificationRepository.GetByUserIdAsync(userId, unreadOnly);
        var unreadCount = await notificationRepository.GetUnreadCountAsync(userId);
        var totalCount = all.Count;

        var paged = all
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapResponse)
            .ToList();

        return new NotificationSummaryResponse(unreadCount, paged, page, pageSize, totalCount);
    }

    public async Task<(NotificationResponse? Notification, string? ErrorCode)> MarkAsReadAsync(
        Guid userId, Guid notificationId)
    {
        var existing = await notificationRepository.GetByIdAsync(notificationId);
        if (existing is null) return (null, "NOTIFICATION_NOT_FOUND");
        if (existing.UserId != userId) return (null, "UNAUTHORIZED");

        var updated = await notificationRepository.MarkAsReadAsync(notificationId);
        return (MapResponse(updated!), null);
    }

    public Task<int> MarkAllAsReadAsync(Guid userId) =>
        notificationRepository.MarkAllAsReadAsync(userId);

    public Task<int> GetUnreadCountAsync(Guid userId) =>
        notificationRepository.GetUnreadCountAsync(userId);

    // Notification timestamps are always UTC instants (set via DateTime.UtcNow), but
    // Postgres round-trips them with DateTimeKind.Unspecified, and the JSON converter
    // only appends "Z" for Kind.Utc — without it, clients parse the value as local time
    // and are off by the server's UTC offset. Re-tag as Utc here rather than in the
    // shared converter, since other date fields (e.g. business hours) are intentionally
    // Unspecified/local.
    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    private static DateTime? EnsureUtc(DateTime? dt) => dt is null ? null : EnsureUtc(dt.Value);

    private static NotificationResponse MapResponse(Notification n) =>
        new(n.Id, n.UserId, n.Type, n.Title, n.Body,
            n.EntityId, n.EntityType, n.Metadata,
            n.IsRead, EnsureUtc(n.ReadAt), EnsureUtc(n.CreatedAt));
}