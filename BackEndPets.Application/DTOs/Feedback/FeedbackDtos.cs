namespace BackEndPets.Application.DTOs.Feedback;

public sealed record CreateWalkerFeedbackRequest(
    Guid WalkerId,
    int Rating,
    string? Comment);

public sealed record CreateBusinessFeedbackRequest(
    Guid BusinessId,
    int Rating,
    string? Comment);

public sealed record FeedbackResponse(
    Guid Id,
    Guid UserId,
    Guid TargetId,
    string TargetType,
    int Rating,
    string? Comment,
    DateTime CreatedAt,
    string UserName,
    string? UserAvatarUrl);

public sealed record UpdateFeedbackRequest(
    int Rating,
    string? Comment);

public sealed record FeedbackSummaryResponse(
    Guid TargetId,
    string TargetType,
    double AverageRating,
    int TotalReviews,
    IReadOnlyCollection<FeedbackResponse> Reviews);