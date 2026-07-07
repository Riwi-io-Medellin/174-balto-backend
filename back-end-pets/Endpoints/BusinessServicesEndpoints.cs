using System.Security.Claims;
using BackEndPets.Application.DTOs.Businesses;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class BusinessServicesEndpoints
{
    public static IEndpointRouteBuilder MapBusinessServicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/businesses")
            .WithTags("Businesses")
            .RequireAuthorization();

        group.MapPost("/{businessId:guid}/services", async (
            Guid businessId,
            CreateBusinessServiceRequest request,
            HttpContext ctx,
            IBusinessServicesService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (created, errorCode) = await service.CreateAsync(userId, businessId, request);
            return errorCode switch
            {
                "BUSINESS_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Business not found.", "BUSINESS_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this business.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_PRICE" => Results.BadRequest(
                    new ApiErrorResponse("Price must be 0 or greater.", "INVALID_PRICE")),
                "INVALID_ITEM_KIND" => Results.BadRequest(
                    new ApiErrorResponse("itemKind must be 'service' or 'product'.", "INVALID_ITEM_KIND")),
                _ when created is not null => Results.Created(
                    $"/api/businesses/{businessId}/services/{created.Id}", created),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CreateBusinessService")
        .WithSummary("Add a service to a business (owner only)")
        .Produces<BusinessServiceResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{businessId:guid}/services", async (
            Guid businessId,
            IBusinessServicesService service) =>
            Results.Ok(await service.GetByBusinessIdAsync(businessId)))
        .WithName("GetBusinessServices")
        .WithSummary("Get all services for a business")
        .Produces<IReadOnlyCollection<BusinessServiceResponse>>(StatusCodes.Status200OK);

        group.MapPut("/{businessId:guid}/services/{serviceId:guid}", async (
            Guid businessId,
            Guid serviceId,
            UpdateBusinessServiceRequest request,
            HttpContext ctx,
            IBusinessServicesService service) =>
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
                    new ApiErrorResponse("You do not own this business.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                "INVALID_PRICE" => Results.BadRequest(
                    new ApiErrorResponse("Price must be 0 or greater.", "INVALID_PRICE")),
                "INVALID_ITEM_KIND" => Results.BadRequest(
                    new ApiErrorResponse("itemKind must be 'service' or 'product'.", "INVALID_ITEM_KIND")),
                _ => Results.Ok(updated)
            };
        })
        .WithName("UpdateBusinessService")
        .WithSummary("Update a service of a business (owner only)")
        .Produces<BusinessServiceResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{businessId:guid}/services/{serviceId:guid}", async (
            Guid businessId,
            Guid serviceId,
            HttpContext ctx,
            IBusinessServicesService service) =>
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
                    new ApiErrorResponse("You do not own this business.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteBusinessService")
        .WithSummary("Delete a service from a business (owner only)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}