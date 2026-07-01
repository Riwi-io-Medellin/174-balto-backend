using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class HomeServicesEndpoints
{
    public static IEndpointRouteBuilder MapHomeServicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home-services")
            .WithTags("HomeServices")
            .RequireAuthorization();

        group.MapGet("/types", async (IHomeServiceTypeService service) =>
                Results.Ok(await service.GetAllAsync()))
            .WithName("GetHomeServiceTypes")
            .WithSummary("List all active home service types")
            .Produces<IReadOnlyCollection<HomeServiceTypeResponse>>(StatusCodes.Status200OK);

        group.MapPost("/providers", async (HttpContext ctx, IHomeServiceProviderService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (provider, errorCode) = await service.BecomeProviderAsync(userId);
            return errorCode switch
            {
                "PROVIDER_ALREADY_EXISTS" => Results.Conflict(
                    new ApiErrorResponse("User is already registered as a home service provider.", "PROVIDER_ALREADY_EXISTS")),
                _ when provider is not null => Results.Created($"/api/home-services/providers/{provider.Id}", provider),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("BecomeHomeServiceProvider")
        .WithSummary("Register the current user as a home service provider")
        .Produces<HomeServiceProviderResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/providers", async (
                    [AsParameters] HomeServiceProviderFilterRequest filters,
                    IHomeServiceProviderService service) =>
                Results.Ok(await service.GetProvidersAsync(filters)))
            .WithName("GetHomeServiceProviders")
            .WithSummary("List all home service providers")
            .Produces<IReadOnlyCollection<HomeServiceProviderResponse>>(StatusCodes.Status200OK);

        group.MapGet("/providers/search", async (
            [AsParameters] HomeServiceSearchQuery query,
            IHomeServiceMarketplaceService service) =>
        {
            var (result, errorCode) = await service.SearchAsync(query);
            return errorCode switch
            {
                "INVALID_DURATION" => Results.BadRequest(new ApiErrorResponse("durationMinutes must be 30, 60, or 90.", "INVALID_DURATION")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("SearchHomeServiceProviders")
        .WithSummary("Search approved home service providers by location, service type, date and duration")
        .Produces<PagedResult<HomeServiceProviderSummaryResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/providers/me", async (HttpContext ctx, IHomeServiceProviderService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (profile, errorCode) = await service.GetMyProviderProfileAsync(userId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                _ when profile is not null => Results.Ok(profile),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyHomeServiceProviderProfile")
        .WithSummary("Get the authenticated user's home service provider profile")
        .Produces<HomeServiceProviderProfileResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/providers/me", async (
            HttpContext ctx, UpdateHomeServiceProviderRequest request, IHomeServiceProviderService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (profile, errorCode) = await service.UpdateMyProviderProfileAsync(userId, request);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Home service provider profile not found.", "PROVIDER_NOT_FOUND")),
                "PROVIDER_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved providers can update their profile.", "PROVIDER_NOT_APPROVED")),
                "YEARS_OF_EXPERIENCE_INVALID" => Results.BadRequest(
                    new ApiErrorResponse("yearsOfExperience must be >= 0.", "YEARS_OF_EXPERIENCE_INVALID")),
                "MAX_CONCURRENT_BOOKINGS_INVALID" => Results.BadRequest(
                    new ApiErrorResponse("maxConcurrentBookings must be >= 1.", "MAX_CONCURRENT_BOOKINGS_INVALID")),
                _ when profile is not null => Results.Ok(profile),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("UpdateMyHomeServiceProviderProfile")
        .WithSummary("Update the authenticated user's home service provider profile (approved providers only)")
        .Produces<HomeServiceProviderProfileResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/providers/{providerId:guid}", async (
            Guid providerId,
            DateOnly? date,
            int? durationMinutes,
            IHomeServiceMarketplaceService service) =>
        {
            var (result, errorCode) = await service.GetDetailAsync(providerId, date, durationMinutes);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Home service provider not found.", "PROVIDER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetHomeServiceProviderDetail")
        .WithSummary("Get full home service provider profile with services, availability and rating")
        .Produces<HomeServiceProviderDetailResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/providers/{providerId:guid}/available-slots", async (
            Guid providerId,
            DateOnly date,
            int durationMinutes,
            IHomeProviderAvailabilityService availabilityService) =>
        {
            var (result, errorCode) = await availabilityService.GetAvailableSlotsAsync(providerId, date, durationMinutes);
            return errorCode switch
            {
                "INVALID_DURATION"                 => Results.BadRequest(new ApiErrorResponse("durationMinutes must be 30, 60, or 90.", "INVALID_DURATION")),
                "PROVIDER_NOT_FOUND"                => Results.NotFound(new ApiErrorResponse("Home service provider not found.", "PROVIDER_NOT_FOUND")),
                "PROVIDER_NOT_APPROVED"             => Results.Conflict(new ApiErrorResponse("Provider is not approved.", "PROVIDER_NOT_APPROVED")),
                "PROVIDER_NOT_ACCEPTING_BOOKINGS"   => Results.Conflict(new ApiErrorResponse("Provider is not accepting bookings.", "PROVIDER_NOT_ACCEPTING_BOOKINGS")),
                _ when result is not null           => Results.Ok(result),
                _                                   => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetHomeServiceProviderAvailableSlots")
        .WithSummary("Get available booking slots for a home service provider on a given date")
        .Produces<IReadOnlyCollection<HomeServiceAvailableSlotResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/providers/apply", async (HttpContext ctx, IHomeServiceApplicationService service) =>
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

            var baseLocation = form["baseLocation"].ToString();
            var experience = form["experience"].ToString();

            if (string.IsNullOrWhiteSpace(baseLocation))
                return Results.BadRequest(
                    new ApiErrorResponse("baseLocation is required.", "BASE_LOCATION_REQUIRED"));

            if (string.IsNullOrWhiteSpace(experience))
                return Results.BadRequest(
                    new ApiErrorResponse("experience is required.", "EXPERIENCE_REQUIRED"));

            var description = form["description"].FirstOrDefault();
            var applyRequest = new HomeServiceApplyRequest(baseLocation, experience, description);

            await using var stream = documentFile.OpenReadStream();
            var (result, errorCode) = await service.ApplyAsync(userId, stream, documentFile.FileName, applyRequest);

            return errorCode switch
            {
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ApplyAsHomeServiceProvider")
        .WithSummary("Submit identity document and profile info to apply as a verified home service provider")
        .Produces<HomeServiceApplyResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
        .DisableAntiforgery();

        return app;
    }
}
