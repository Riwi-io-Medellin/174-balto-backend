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
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Nit) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                request.Phone <= 0)
            {
                return Results.BadRequest(new ApiErrorResponse(
                    "Name, nit, email, and a valid phone number are required.",
                    "VALIDATION_FAILED"));
            }

            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (business, errorCode) = await service.CreateBusinessAsync(userId, request);
            return errorCode switch
            {
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
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("CreateBusiness")
        .WithSummary("Register a new business for the current user")
        .Produces<BusinessResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }
}
