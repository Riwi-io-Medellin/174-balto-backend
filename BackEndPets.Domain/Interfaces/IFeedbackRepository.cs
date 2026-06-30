using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IFeedbackRepository
{
    Task<Feedback> CreateAsync(Feedback feedback);
    Task<Feedback?> GetByIdAsync(Guid id);
    Task<FeedbackWithUserProjection?> GetByIdWithUserAsync(Guid id);
    Task<IReadOnlyCollection<Feedback>> GetByTargetAsync(Guid targetId, string targetType);
    Task<IReadOnlyCollection<FeedbackWithUserProjection>> GetByTargetWithUserAsync(Guid targetId, string targetType);
    Task<bool> ExistsAsync(Guid userId, Guid targetId, string targetType);
    Task<Feedback> UpdateAsync(Feedback feedback);
    Task DeleteAsync(Feedback feedback);
}