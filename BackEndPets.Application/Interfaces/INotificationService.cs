using BackEndPets.Application.DTOs.Notifications;

namespace BackEndPets.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(CreateNotificationRequest request);
    Task<NotificationSummaryResponse> GetMyNotificationsAsync(Guid userId, bool? unreadOnly = null, int page = 1, int pageSize = 20);
    Task<(NotificationResponse? Notification, string? ErrorCode)> MarkAsReadAsync(Guid userId, Guid notificationId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}
