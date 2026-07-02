using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class HomeProviderServicesEndpoints
{
    public static IEndpointRouteBuilder MapHomeProviderServicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home-services/providers")
            .WithTags("HomeServices")
            .RequireAuthorization();

        group.MapGet("/{providerId:guid}/services", async (Guid providerId, IHomeProviderServicesService service) =>
                Results.Ok(await service.GetByProviderIdAsync(providerId)))
            .WithName("GetHomeProviderServices")
            .WithSummary("Get all offered services for a home service provider")
            .Produces<IReadOnlyCollection<HomeProviderServiceResponse>>(StatusCodes.Status200OK);

        group.MapPost("/me/services", async (
            AddHomeProviderServiceRequest request,
            HttpContext ctx,
            IHomeProviderServicesService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (created, errorCode) = await service.AddAsync(userId, request);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                "SERVICE_TYPE_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Service type not found.", "SERVICE_TYPE_NOT_FOUND")),
                "SERVICE_TYPE_ALREADY_OFFERED" => Results.Conflict(
                    new ApiErrorResponse("You already offer this service type.", "SERVICE_TYPE_ALREADY_OFFERED")),
                "INVALID_PRICE" => Results.BadRequest(
                    new ApiErrorResponse("Price must be 0 or greater.", "INVALID_PRICE")),
                "INVALID_PRICE_UNIT" => Results.BadRequest(
                    new ApiErrorResponse("priceUnit must be flat, hourly, or per_visit.", "INVALID_PRICE_UNIT")),
                _ when created is not null => Results.Created($"/api/home-services/providers/me/services/{created.Id}", created),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddHomeProviderService")
        .WithSummary("Add an offered service to the current provider")
        .Produces<HomeProviderServiceResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPut("/me/services/{serviceId:guid}", async (
            Guid serviceId,
            UpdateHomeProviderServiceRequest request,
            HttpContext ctx,
            IHomeProviderServicesService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (updated, errorCode) = await service.UpdateAsync(userId, serviceId, request);
            return errorCode switch
            {
                "SERVICE_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Service not found.", "SERVICE_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this service.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_PRICE" => Results.BadRequest(
                    new ApiErrorResponse("Price must be 0 or greater.", "INVALID_PRICE")),
                "INVALID_PRICE_UNIT" => Results.BadRequest(
                    new ApiErrorResponse("priceUnit must be flat, hourly, or per_visit.", "INVALID_PRICE_UNIT")),
                _ when updated is not null => Results.Ok(updated),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("UpdateHomeProviderService")
        .WithSummary("Update an offered service of the current provider")
        .Produces<HomeProviderServiceResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/me/services/{serviceId:guid}", async (
            Guid serviceId,
            HttpContext ctx,
            IHomeProviderServicesService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteAsync(userId, serviceId);
            return errorCode switch
            {
                "SERVICE_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Service not found.", "SERVICE_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this service.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ when success => Results.NoContent(),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("DeleteHomeProviderService")
        .WithSummary("Remove an offered service from the current provider")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
