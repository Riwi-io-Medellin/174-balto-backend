using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Feedback;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class FeedbackEndpoints
{
    public static IEndpointRouteBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feedback")
            .WithTags("Feedback")
            .RequireAuthorization();

        group.MapPost("/walkers", async (
            CreateWalkerFeedbackRequest request,
            HttpContext ctx,
            IFeedbackService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (feedback, errorCode) = await service.CreateWalkerFeedbackAsync(userId, request);
            return errorCode switch
            {
                "INVALID_RATING" => Results.BadRequest(
                    new ApiErrorResponse("Rating must be between 1 and 5.", "INVALID_RATING")),
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker not found.", "WALKER_NOT_FOUND")),
                "NO_WALK_HISTORY" => Results.Json(
                    new ApiErrorResponse("You have not had a walk with this walker.", "NO_WALK_HISTORY"),
                    statusCode: StatusCodes.Status403Forbidden),
                "FEEDBACK_ALREADY_EXISTS" => Results.Conflict(
                    new ApiErrorResponse("You have already reviewed this walker.", "FEEDBACK_ALREADY_EXISTS")),
                _ when feedback is not null => Results.Created($"/api/feedback/walkers/{feedback.Id}", feedback),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CreateWalkerFeedback")
        .WithSummary("Leave feedback for a walker (requires walk history)")
        .Produces<FeedbackResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/businesses", async (
            CreateBusinessFeedbackRequest request,
            HttpContext ctx,
            IFeedbackService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (feedback, errorCode) = await service.CreateBusinessFeedbackAsync(userId, request);
            return errorCode switch
            {
                "INVALID_RATING" => Results.BadRequest(
                    new ApiErrorResponse("Rating must be between 1 and 5.", "INVALID_RATING")),
                "BUSINESS_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Business not found.", "BUSINESS_NOT_FOUND")),
                "FEEDBACK_ALREADY_EXISTS" => Results.Conflict(
                    new ApiErrorResponse("You have already reviewed this business.", "FEEDBACK_ALREADY_EXISTS")),
                _ when feedback is not null => Results.Created($"/api/feedback/businesses/{feedback.Id}", feedback),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CreateBusinessFeedback")
        .WithSummary("Leave feedback for a business (any authenticated user)")
        .Produces<FeedbackResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/walkers/{walkerId:guid}", async (Guid walkerId, IFeedbackService service) =>
        {
            var summary = await service.GetByWalkerAsync(walkerId);
            return summary is null
                ? Results.NotFound(new ApiErrorResponse("Walker not found.", "WALKER_NOT_FOUND"))
                : Results.Ok(summary);
        })
        .WithName("GetWalkerFeedback")
        .WithSummary("Get all feedback and average rating for a walker")
        .Produces<FeedbackSummaryResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/businesses/{businessId:guid}", async (Guid businessId, IFeedbackService service) =>
        {
            var summary = await service.GetByBusinessAsync(businessId);
            return summary is null
                ? Results.NotFound(new ApiErrorResponse("Business not found.", "BUSINESS_NOT_FOUND"))
                : Results.Ok(summary);
        })
        .WithName("GetBusinessFeedback")
        .WithSummary("Get all feedback and average rating for a business")
        .Produces<FeedbackSummaryResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}