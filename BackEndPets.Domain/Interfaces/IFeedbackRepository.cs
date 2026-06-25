using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IFeedbackRepository
{
    Task<Feedback> CreateAsync(Feedback feedback);
    Task<IReadOnlyCollection<Feedback>> GetByTargetAsync(Guid targetId, string targetType);
    Task<bool> ExistsAsync(Guid userId, Guid targetId, string targetType);
}