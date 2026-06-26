using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId, bool? unreadOnly = null);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification?> MarkAsReadAsync(Guid id);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}