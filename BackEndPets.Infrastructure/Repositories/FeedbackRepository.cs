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

    public async Task<IReadOnlyCollection<Feedback>> GetByTargetAsync(Guid targetId, string targetType) =>
        await dbContext.Feedbacks
            .Where(f => f.TargetId == targetId && f.TargetType == targetType)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public Task<bool> ExistsAsync(Guid userId, Guid targetId, string targetType) =>
        dbContext.Feedbacks.AnyAsync(f =>
            f.UserId == userId &&
            f.TargetId == targetId &&
            f.TargetType == targetType);
}