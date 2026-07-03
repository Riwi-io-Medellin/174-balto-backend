using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Profiles;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class BusinessesEndpoints
{
    public static IEndpointRouteBuilder MapBusinessesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/businesses")
            .WithTags("Businesses")
            .RequireAuthorization();

        group.MapPost("/", async (CreateBusinessRequest request, HttpContext ctx, IProfileService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (business, errorCode) = await service.CreateBusinessAsync(userId, request);
            return errorCode switch
            {
                "VALIDATION_FAILED" => Results.BadRequest(new ApiErrorResponse(
                    "Name, nit, email, and a valid phone number are required.",
                    "VALIDATION_FAILED")),
                "INVALID_BUSINESS_TYPE" => Results.BadRequest(new ApiErrorResponse(
                    "type must be one of: veterinary, grooming, shelter, petshop, other.",
                    "INVALID_BUSINESS_TYPE")),
                "NIT_ALREADY_TAKEN" => Results.Conflict(new ApiErrorResponse(
                    "A business with this NIT already exists.",
                    "NIT_ALREADY_TAKEN")),
                "EMAIL_ALREADY_TAKEN" => Results.Conflict(new ApiErrorResponse(
                    "A business with this email already exists.",
                    "EMAIL_ALREADY_TAKEN")),
                _ when business is not null => Results.Created($"/api/businesses/{business.Id}", business),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CreateBusiness")
        .WithSummary("Register a new business for the current user")
        .Produces<BusinessResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/", async (
                    [AsParameters] BusinessFilterRequest filters,
                    IProfileService service) =>
                Results.Ok(await service.GetBusinessesAsync(filters.Type, filters.Location)))
            .WithName("GetBusinesses")
            .WithSummary("List all businesses")
            .Produces<IReadOnlyCollection<BusinessResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IProfileService service) =>
            {
                var business = await service.GetBusinessByIdAsync(id);
                return business is null
                    ? Results.NotFound(new ApiErrorResponse("Business not found.", "BUSINESS_NOT_FOUND"))
                    : Results.Ok(business);
            })
            .WithName("GetBusinessById")
            .WithSummary("Get a business by id")
            .Produces<BusinessResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/me", async (UpdateBusinessRequest request, HttpContext ctx, IProfileService service) =>
            {
                var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var (business, errorCode) = await service.UpdateMyBusinessAsync(userId, request);
                return errorCode switch
                {
                    "BUSINESS_NOT_FOUND" => Results.NotFound(new ApiErrorResponse(
                        "Business profile not found.", "BUSINESS_NOT_FOUND")),
                    "BUSINESS_NOT_APPROVED" => Results.Conflict(new ApiErrorResponse(
                        "Only approved businesses can update their profile.", "BUSINESS_NOT_APPROVED")),
                    _ when business is not null => Results.Ok(business),
                    _ => ResultsExtensions.UnhandledError()
                };
            })
            .WithName("UpdateMyBusiness")
            .Produces<BusinessResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/me", async (HttpContext ctx, IProfileService service) =>
            {
                var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var business = await service.GetMyBusinessAsync(userId);
                return business is null
                    ? Results.NotFound(new ApiErrorResponse(
                        "You have not registered a business yet.", "BUSINESS_NOT_FOUND"))
                    : Results.Ok(business);
            })
            .WithName("GetMyBusiness")
            .Produces<BusinessResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
