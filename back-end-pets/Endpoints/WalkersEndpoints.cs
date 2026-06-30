using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Profiles;
using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEndPets.API.Endpoints;

public static class WalkersEndpoints
{
    public static IEndpointRouteBuilder MapWalkersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/walkers")
            .WithTags("Walkers")
            .RequireAuthorization();

        group.MapPost("/", async (HttpContext ctx, IProfileService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (walker, errorCode) = await service.BecomeWalkerAsync(userId);
            return errorCode switch
            {
                "WALKER_ALREADY_EXISTS" => Results.Conflict(
                    new ApiErrorResponse("User is already registered as a walker.", "WALKER_ALREADY_EXISTS")),
                _ when walker is not null => Results.Created($"/api/walkers/{walker.Id}", walker),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("BecomeWalker")
        .WithSummary("Register the current user as a walker")
        .Produces<WalkerResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/", async (
                    [AsParameters] WalkerFilterRequest filters,
                    IProfileService service) =>
                Results.Ok(await service.GetWalkersAsync(filters.Available, filters.WorkLocation)))
            .WithName("GetWalkers")
            .WithSummary("List all walkers")
            .Produces<IReadOnlyCollection<WalkerResponse>>(StatusCodes.Status200OK);

        group.MapGet("/search", async (
            [AsParameters] WalkerSearchQuery query,
            IWalkerMarketplaceService service) =>
        {
            var (result, errorCode) = await service.SearchAsync(query);
            return errorCode switch
            {
                "INVALID_DURATION" => Results.BadRequest(new ApiErrorResponse("durationMinutes must be 30, 60, or 90.", "INVALID_DURATION")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("SearchWalkers")
        .WithSummary("Search approved walkers by location, date and duration")
        .Produces<PagedResult<WalkerSummaryResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/{walkerId:guid}", async (
            Guid walkerId,
            DateOnly? date,
            int? durationMinutes,
            IWalkerMarketplaceService service) =>
        {
            var (result, errorCode) = await service.GetDetailAsync(walkerId, date, durationMinutes);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Walker not found.", "WALKER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetWalkerDetail")
        .WithSummary("Get full walker profile with availability and rating")
        .Produces<WalkerDetailResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
        
        group.MapGet("/me", async (HttpContext ctx, IProfileService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (profile, errorCode) = await service.GetMyWalkerProfileAsync(userId);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                _ when profile is not null => Results.Ok(profile),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyWalkerProfile")
        .WithSummary("Get the authenticated user's walker profile")
        .Produces<WalkerProfileResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/me", async (HttpContext ctx, UpdateWalkerProfileRequest request, IProfileService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (profile, errorCode) = await service.UpdateMyWalkerProfileAsync(userId, request);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Walker profile not found.", "WALKER_NOT_FOUND")),
                "WALKER_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved walkers can update their profile.", "WALKER_NOT_APPROVED")),
                "HOURLY_RATE_INVALID" => Results.BadRequest(
                    new ApiErrorResponse("hourlyRate must be >= 0.", "HOURLY_RATE_INVALID")),
                "SERVICE_RADIUS_INVALID" => Results.BadRequest(
                    new ApiErrorResponse("serviceRadiusKm must be > 0.", "SERVICE_RADIUS_INVALID")),
                "YEARS_OF_EXPERIENCE_INVALID" => Results.BadRequest(
                    new ApiErrorResponse("yearsOfExperience must be >= 0.", "YEARS_OF_EXPERIENCE_INVALID")),
                _ when profile is not null => Results.Ok(profile),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("UpdateMyWalkerProfile")
        .WithSummary("Update the authenticated user's walker profile (approved walkers only)")
        .Produces<WalkerProfileResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/recommendations", async (
                    [AsParameters] WalkerRecommendationRequest request,
                    IProfileService service) =>
                Results.Ok(await service.GetWalkerRecommendationsAsync(request)))
            .WithName("GetWalkerRecommendations")
            .WithSummary("Get recommended walkers ordered by availability, location and experience")
            .Produces<IReadOnlyCollection<WalkerRecommendationResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{walkerId:guid}/available-slots", async (
            Guid walkerId,
            DateOnly date,
            int durationMinutes,
            IWalkerAvailabilityService availabilityService) =>
        {
            var (result, errorCode) = await availabilityService.GetAvailableSlotsAsync(walkerId, date, durationMinutes);
            return errorCode switch
            {
                "INVALID_DURATION"              => Results.BadRequest(new ApiErrorResponse("durationMinutes must be 30, 60, or 90.", "INVALID_DURATION")),
                "WALKER_NOT_FOUND"              => Results.NotFound(new ApiErrorResponse("Walker not found.", "WALKER_NOT_FOUND")),
                "WALKER_NOT_APPROVED"           => Results.Conflict(new ApiErrorResponse("Walker is not approved.", "WALKER_NOT_APPROVED")),
                "WALKER_NOT_ACCEPTING_BOOKINGS" => Results.Conflict(new ApiErrorResponse("Walker is not accepting bookings.", "WALKER_NOT_ACCEPTING_BOOKINGS")),
                _ when result is not null       => Results.Ok(result),
                _                               => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetWalkerAvailableSlots")
        .WithSummary("Get available booking slots for a walker on a given date")
        .Produces<IReadOnlyCollection<AvailableSlotResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/apply", async (HttpContext ctx, IWalkerApplicationService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            if (!ctx.Request.HasFormContentType)
                return Results.BadRequest(
                    new ApiErrorResponse("Expected multipart/form-data.", "INVALID_CONTENT_TYPE"));

            var form = await ctx.Request.ReadFormAsync();
            var documentFile = form.Files.GetFile("document");

            if (documentFile is null || documentFile.Length == 0)
                return Results.BadRequest(
                    new ApiErrorResponse("Identity document image is required.", "DOCUMENT_REQUIRED"));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return Results.BadRequest(
                    new ApiErrorResponse("Invalid file type. Allowed: jpg, jpeg, png, webp.", "INVALID_FILE_TYPE"));

            if (documentFile.Length > 10 * 1024 * 1024)
                return Results.BadRequest(
                    new ApiErrorResponse("File exceeds 10 MB limit.", "FILE_TOO_LARGE"));

            var workLocation = form["workLocation"].ToString();
            var experience = form["experience"].ToString();

            if (string.IsNullOrWhiteSpace(workLocation))
                return Results.BadRequest(
                    new ApiErrorResponse("workLocation is required.", "WORK_LOCATION_REQUIRED"));

            if (string.IsNullOrWhiteSpace(experience))
                return Results.BadRequest(
                    new ApiErrorResponse("experience is required.", "EXPERIENCE_REQUIRED"));

            var description = form["description"].FirstOrDefault();
            var applyRequest = new WalkerApplyRequest(workLocation, experience, description);

            await using var stream = documentFile.OpenReadStream();
            var (result, errorCode) = await service.ApplyAsync(userId, stream, documentFile.FileName, applyRequest);

            return errorCode switch
            {
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ApplyAsWalker")
        .WithSummary("Submit identity document and profile info to apply as a verified walker")
        .Produces<WalkerApplyResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
        .DisableAntiforgery();

        return app;
    }
}
