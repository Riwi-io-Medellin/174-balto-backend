using BackEndPets.Application.DTOs.Feedback;

namespace BackEndPets.Application.Interfaces;

public interface IFeedbackService
{
    Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateWalkerFeedbackAsync(
        Guid userId, CreateWalkerFeedbackRequest request);

    Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateBusinessFeedbackAsync(
        Guid userId, CreateBusinessFeedbackRequest request);

    Task<(FeedbackResponse? Feedback, string? ErrorCode)> UpdateFeedbackAsync(
        Guid userId, Guid feedbackId, UpdateFeedbackRequest request);

    Task<(bool Success, string? ErrorCode)> DeleteFeedbackAsync(Guid userId, Guid feedbackId);

    Task<FeedbackSummaryResponse?> GetByWalkerAsync(Guid walkerId);
    Task<FeedbackSummaryResponse?> GetByBusinessAsync(Guid businessId);
}