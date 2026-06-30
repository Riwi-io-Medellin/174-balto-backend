using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class FeedbackRepository(AppIdentityDbContext dbContext) : IFeedbackRepository
{
    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        feedback.Id = Guid.NewGuid();
        feedback.CreatedAt = DateTime.UtcNow;
        dbContext.Feedbacks.Add(feedback);
        await dbContext.SaveChangesAsync();
        return feedback;
    }

    public Task<Feedback?> GetByIdAsync(Guid id) =>
        dbContext.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);

    public async Task<FeedbackWithUserProjection?> GetByIdWithUserAsync(Guid id) =>
        await dbContext.Feedbacks
            .Where(f => f.Id == id)
            .Join(dbContext.Users,
                f => f.UserId,
                u => u.Id,
                (f, u) => new FeedbackWithUserProjection(f, u.FirstName, u.LastName, u.PhotoUrl))
            .FirstOrDefaultAsync();

    public async Task<IReadOnlyCollection<Feedback>> GetByTargetAsync(Guid targetId, string targetType) =>
        await dbContext.Feedbacks
            .Where(f => f.TargetId == targetId && f.TargetType == targetType)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyCollection<FeedbackWithUserProjection>> GetByTargetWithUserAsync(Guid targetId, string targetType) =>
        await dbContext.Feedbacks
            .Where(f => f.TargetId == targetId && f.TargetType == targetType)
            .Join(dbContext.Users,
                f => f.UserId,
                u => u.Id,
                (f, u) => new FeedbackWithUserProjection(f, u.FirstName, u.LastName, u.PhotoUrl))
            .OrderByDescending(f => f.Feedback.CreatedAt)
            .ToListAsync();

    public Task<bool> ExistsAsync(Guid userId, Guid targetId, string targetType) =>
        dbContext.Feedbacks.AnyAsync(f =>
            f.UserId == userId &&
            f.TargetId == targetId &&
            f.TargetType == targetType);

    public async Task<Feedback> UpdateAsync(Feedback feedback)
    {
        dbContext.Feedbacks.Update(feedback);
        await dbContext.SaveChangesAsync();
        return feedback;
    }

    public async Task DeleteAsync(Feedback feedback)
    {
        dbContext.Feedbacks.Remove(feedback);
        await dbContext.SaveChangesAsync();
    }
}