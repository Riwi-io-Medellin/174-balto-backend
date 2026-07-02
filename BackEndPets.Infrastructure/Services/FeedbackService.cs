using BackEndPets.Application.DTOs.Feedback;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class FeedbackService(
    IFeedbackRepository feedbackRepository,
    IWalkerRepository walkerRepository,
    IBusinessRepository businessRepository,
    IWalkBookingRepository bookingRepository,
    IHomeServiceProviderRepository homeServiceProviderRepository,
    IHomeServiceBookingRepository homeServiceBookingRepository) : IFeedbackService
{
    public async Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateWalkerFeedbackAsync(
        Guid userId, CreateWalkerFeedbackRequest request)
    {
        if (request.Rating is < 1 or > 5)
            return (null, "INVALID_RATING");

        var walker = await walkerRepository.GetByIdAsync(request.WalkerId);
        if (walker is null) return (null, "WALKER_NOT_FOUND");

        var bookings = await bookingRepository.GetByClientUserIdAsync(userId, status: "completed");
        var hadWalk = bookings.Any(b => b.WalkerId == request.WalkerId);
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
        return await MapCreatedResponse(created.Id);
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
        return await MapCreatedResponse(created.Id);
    }

    public async Task<(FeedbackResponse? Feedback, string? ErrorCode)> CreateHomeServiceProviderFeedbackAsync(
        Guid userId, CreateHomeServiceProviderFeedbackRequest request)
    {
        if (request.Rating is < 1 or > 5)
            return (null, "INVALID_RATING");

        var provider = await homeServiceProviderRepository.GetByIdAsync(request.ProviderId);
        if (provider is null) return (null, "PROVIDER_NOT_FOUND");

        var bookings = await homeServiceBookingRepository.GetByClientUserIdAsync(userId, status: "completed");
        var hadService = bookings.Any(b => b.ProviderId == request.ProviderId);
        if (!hadService) return (null, "NO_SERVICE_HISTORY");

        if (await feedbackRepository.ExistsAsync(userId, request.ProviderId, "home_service_provider"))
            return (null, "FEEDBACK_ALREADY_EXISTS");

        var feedback = new Feedback
        {
            UserId = userId,
            TargetId = request.ProviderId,
            TargetType = "home_service_provider",
            Rating = request.Rating,
            Comment = request.Comment?.Trim()
        };

        var created = await feedbackRepository.CreateAsync(feedback);
        return await MapCreatedResponse(created.Id);
    }

    public async Task<(FeedbackResponse? Feedback, string? ErrorCode)> UpdateFeedbackAsync(
        Guid userId, Guid feedbackId, UpdateFeedbackRequest request)
    {
        if (request.Rating is < 1 or > 5)
            return (null, "INVALID_RATING");

        var existing = await feedbackRepository.GetByIdAsync(feedbackId);
        if (existing is null) return (null, "FEEDBACK_NOT_FOUND");

        if (existing.UserId != userId) return (null, "NOT_FEEDBACK_OWNER");

        existing.Rating = request.Rating;
        existing.Comment = request.Comment?.Trim();

        var updated = await feedbackRepository.UpdateAsync(existing);
        return await MapCreatedResponse(updated.Id);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteFeedbackAsync(Guid userId, Guid feedbackId)
    {
        var existing = await feedbackRepository.GetByIdAsync(feedbackId);
        if (existing is null) return (false, "FEEDBACK_NOT_FOUND");

        if (existing.UserId != userId) return (false, "NOT_FEEDBACK_OWNER");

        await feedbackRepository.DeleteAsync(existing);
        return (true, null);
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

    public async Task<FeedbackSummaryResponse?> GetByHomeServiceProviderAsync(Guid providerId)
    {
        var provider = await homeServiceProviderRepository.GetByIdAsync(providerId);
        if (provider is null) return null;

        return await BuildSummaryAsync(providerId, "home_service_provider");
    }

    private async Task<FeedbackSummaryResponse> BuildSummaryAsync(Guid targetId, string targetType)
    {
        var feedbacks = await feedbackRepository.GetByTargetWithUserAsync(targetId, targetType);
        var average = feedbacks.Count > 0 ? feedbacks.Average(f => f.Feedback.Rating) : 0.0;

        return new FeedbackSummaryResponse(
            targetId,
            targetType,
            Math.Round(average, 1),
            feedbacks.Count,
            feedbacks.Select(MapResponse).ToList());
    }

    private async Task<(FeedbackResponse? Feedback, string? ErrorCode)> MapCreatedResponse(Guid feedbackId)
    {
        var projection = await feedbackRepository.GetByIdWithUserAsync(feedbackId);
        if (projection is null) return (null, "FEEDBACK_NOT_FOUND");

        return (MapResponse(projection), null);
    }

    private static FeedbackResponse MapResponse(FeedbackWithUserProjection p) =>
        new(p.Feedback.Id, p.Feedback.UserId, p.Feedback.TargetId, p.Feedback.TargetType,
            p.Feedback.Rating, p.Feedback.Comment, p.Feedback.CreatedAt,
            $"{p.FirstName} {p.LastName}", p.PhotoUrl);
}