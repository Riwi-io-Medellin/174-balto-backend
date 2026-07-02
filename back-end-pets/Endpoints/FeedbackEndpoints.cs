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
                _ => ResultsExtensions.UnhandledError()
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
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CreateBusinessFeedback")
        .WithSummary("Leave feedback for a business (any authenticated user)")
        .Produces<FeedbackResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/home-service-providers", async (
            CreateHomeServiceProviderFeedbackRequest request,
            HttpContext ctx,
            IFeedbackService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (feedback, errorCode) = await service.CreateHomeServiceProviderFeedbackAsync(userId, request);
            return errorCode switch
            {
                "INVALID_RATING" => Results.BadRequest(
                    new ApiErrorResponse("Rating must be between 1 and 5.", "INVALID_RATING")),
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider not found.", "PROVIDER_NOT_FOUND")),
                "NO_SERVICE_HISTORY" => Results.Json(
                    new ApiErrorResponse("You have not had a completed service with this provider.", "NO_SERVICE_HISTORY"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ when feedback is not null => Results.Created($"/api/feedback/home-service-providers/{feedback.Id}", feedback),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CreateHomeServiceProviderFeedback")
        .WithSummary("Leave feedback for a home service provider (requires completed service history)")
        .Produces<FeedbackResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/home-service-providers/{providerId:guid}", async (Guid providerId, IFeedbackService service) =>
        {
            var summary = await service.GetByHomeServiceProviderAsync(providerId);
            return summary is null
                ? Results.NotFound(new ApiErrorResponse("Home service provider not found.", "PROVIDER_NOT_FOUND"))
                : Results.Ok(summary);
        })
        .WithName("GetHomeServiceProviderFeedback")
        .WithSummary("Get all feedback and average rating for a home service provider")
        .Produces<FeedbackSummaryResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

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

        group.MapPut("/{feedbackId:guid}", async (
            Guid feedbackId,
            UpdateFeedbackRequest request,
            HttpContext ctx,
            IFeedbackService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (feedback, errorCode) = await service.UpdateFeedbackAsync(userId, feedbackId, request);
            return errorCode switch
            {
                "INVALID_RATING" => Results.BadRequest(
                    new ApiErrorResponse("Rating must be between 1 and 5.", "INVALID_RATING")),
                "FEEDBACK_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Feedback not found.", "FEEDBACK_NOT_FOUND")),
                "NOT_FEEDBACK_OWNER" => Results.Json(
                    new ApiErrorResponse("You can only edit your own feedback.", "NOT_FEEDBACK_OWNER"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ when feedback is not null => Results.Ok(feedback),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("UpdateFeedback")
        .WithSummary("Update your own feedback (rating and/or comment)")
        .Produces<FeedbackResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{feedbackId:guid}", async (
            Guid feedbackId,
            HttpContext ctx,
            IFeedbackService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteFeedbackAsync(userId, feedbackId);
            return errorCode switch
            {
                "FEEDBACK_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Feedback not found.", "FEEDBACK_NOT_FOUND")),
                "NOT_FEEDBACK_OWNER" => Results.Json(
                    new ApiErrorResponse("You can only delete your own feedback.", "NOT_FEEDBACK_OWNER"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ when success => Results.NoContent(),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("DeleteFeedback")
        .WithSummary("Delete your own feedback")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}