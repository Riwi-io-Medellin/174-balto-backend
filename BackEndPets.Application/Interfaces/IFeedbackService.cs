using BackEndPets.Application.DTOs.Feedback;

namespace BackEndPets.Application.Interfaces;

public interface IFeedbackService
{
    Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateWalkerFeedbackAsync(
        Guid userId, CreateWalkerFeedbackRequest request);

    Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateBusinessFeedbackAsync(
        Guid userId, CreateBusinessFeedbackRequest request);

    Task<FeedbackSummaryResponse?> GetByWalkerAsync(Guid walkerId);
    Task<FeedbackSummaryResponse?> GetByBusinessAsync(Guid businessId);
}