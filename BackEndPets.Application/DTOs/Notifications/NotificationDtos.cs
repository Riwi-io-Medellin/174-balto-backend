namespace BackEndPets.Application.DTOs.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid UserId,
    string Type,
    string Title,
    string Body,
    Guid? EntityId,
    string? EntityType,
    string? Metadata,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);

public sealed record NotificationSummaryResponse(
    int UnreadCount,
    IReadOnlyCollection<NotificationResponse> Notifications);

public sealed record CreateNotificationRequest(
    Guid UserId,
    string Type,
    string Title,
    string Body,
    Guid? EntityId = null,
    string? EntityType = null,
    string? Metadata = null);