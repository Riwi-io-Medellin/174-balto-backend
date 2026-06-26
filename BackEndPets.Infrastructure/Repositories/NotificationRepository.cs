using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class NotificationRepository(AppIdentityDbContext dbContext) : INotificationRepository
{
    public async Task<Notification> CreateAsync(Notification notification)
    {
        notification.Id = Guid.NewGuid();
        notification.CreatedAt = DateTime.UtcNow;
        notification.IsRead = false;
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();
        return notification;
    }

    public async Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId, bool? unreadOnly = null)
    {
        var query = dbContext.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public Task<Notification?> GetByIdAsync(Guid id) =>
        dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id);

    public async Task<Notification?> MarkAsReadAsync(Guid id)
    {
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        if (notification is null) return null;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return notification;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        var unread = await dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        await dbContext.SaveChangesAsync();
        return unread.Count;
    }

    public Task<int> GetUnreadCountAsync(Guid userId) =>
        dbContext.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
}