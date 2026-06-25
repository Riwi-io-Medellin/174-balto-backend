using BackEndPets.Application.DTOs.Feedback;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class FeedbackService(
    IFeedbackRepository feedbackRepository,
    IWalkerRepository walkerRepository,
    IBusinessRepository businessRepository,
    IPetWalkingHistoryRepository historyRepository) : IFeedbackService
{
    public async Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateWalkerFeedbackAsync(
        Guid userId, CreateWalkerFeedbackRequest request)
    {
        if (request.Rating is < 1 or > 5)
            return (null, "INVALID_RATING");

        var walker = await walkerRepository.GetByIdAsync(request.WalkerId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        // Solo owners que tuvieron paseo con este walker
        var histories = await historyRepository.GetByUserIdAsync(userId);
        var hadWalk = histories.Any(h => h.WalkerId == request.WalkerId);
        if (!hadWalk) return (null, "NO_WALK_HISTORY");

        if (await feedbackRepository.ExistsAsync(userId, request.WalkerId, "walker"))
            return (null, "FEEDBACK_ALREADY_EXISTS");

        var feedback = new Feedback
        {
            UserId = userId,
            TargetId = request.WalkerId,
            TargetType = "walker",
            Rating = request.Rating,
            Comment = request.Comment?.Trim()
        };

        var created = await feedbackRepository.CreateAsync(feedback);
        return (MapResponse(created), null);
    }

    public async Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateBusinessFeedbackAsync(
        Guid userId, CreateBusinessFeedbackRequest request)
    {
        if (request.Rating is < 1 or > 5)
            return (null, "INVALID_RATING");

        var business = await businessRepository.GetByIdAsync(request.BusinessId);
        if (business is null) return (null, "BUSINESS_NOT_FOUND");

        if (await feedbackRepository.ExistsAsync(userId, request.BusinessId, "business"))
            return (null, "FEEDBACK_ALREADY_EXISTS");

        var feedback = new Feedback
        {
            UserId = userId,
            TargetId = request.BusinessId,
            TargetType = "business",
            Rating = request.Rating,
            Comment = request.Comment?.Trim()
        };

        var created = await feedbackRepository.CreateAsync(feedback);
        return (MapResponse(created), null);
    }

    public async Task<FeedbackSummaryResponse?> GetByWalkerAsync(Guid walkerId)
    {
        var walker = await walkerRepository.GetByIdAsync(walkerId);
        if (walker is null) return null;

        return await BuildSummaryAsync(walkerId, "walker");
    }

    public async Task<FeedbackSummaryResponse?> GetByBusinessAsync(Guid businessId)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business is null) return null;

        return await BuildSummaryAsync(businessId, "business");
    }

    private async Task<FeedbackSummaryResponse> BuildSummaryAsync(Guid targetId, string targetType)
    {
        var feedbacks = await feedbackRepository.GetByTargetAsync(targetId, targetType);
        var average = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;

        return new FeedbackSummaryResponse(
            targetId,
            targetType,
            Math.Round(average, 1),
            feedbacks.Count,
            feedbacks.Select(MapResponse).ToList());
    }

    private static FeedbackResponse MapResponse(Feedback f) =>
        new(f.Id, f.UserId, f.TargetId, f.TargetType, f.Rating, f.Comment, f.CreatedAt);
}