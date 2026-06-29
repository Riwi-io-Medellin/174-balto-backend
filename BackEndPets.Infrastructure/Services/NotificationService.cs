using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class NotificationService(
    INotificationRepository notificationRepository) : INotificationService
{
    private static readonly string[] ValidTypes =
    [
        "walk_started", "walk_finished", "walk_cancelled",
        "walker_assigned", "walker_approved", "walker_rejected",
        "business_approved", "business_rejected", "system"
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
        return MapResponse(created);
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

    private static NotificationResponse MapResponse(Notification n) =>
        new(n.Id, n.UserId, n.Type, n.Title, n.Body,
            n.EntityId, n.EntityType, n.Metadata,
            n.IsRead, n.ReadAt, n.CreatedAt);
}