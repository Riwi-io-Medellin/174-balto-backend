using System.Security.Claims;
using BackEndPets.Application.DTOs.Businesses;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class BusinessAssetsEndpoints
{
    public static IEndpointRouteBuilder MapBusinessAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/businesses")
            .WithTags("Businesses")
            .RequireAuthorization();

        group.MapPost("/{businessId:guid}/documents", async (
            Guid businessId,
            AddBusinessDocumentRequest request,
            HttpContext ctx,
            IBusinessAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (document, errorCode) = await service.AddDocumentAsync(userId, businessId, request);
            return errorCode switch
            {
                "BUSINESS_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Business not found.", "BUSINESS_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this business.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ when document is not null => Results.Created(
                    $"/api/businesses/{businessId}/documents/{document.Id}", document),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        })
        .WithName("AddBusinessDocument")
        .WithSummary("Add a document to a business (owner only)")
        .Produces<BusinessDocumentResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{businessId:guid}/documents", async (
            Guid businessId,
            IBusinessAssetsService service) =>
            Results.Ok(await service.GetDocumentsAsync(businessId)))
        .WithName("GetBusinessDocuments")
        .WithSummary("Get all documents for a business")
        .Produces<IReadOnlyCollection<BusinessDocumentResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/{businessId:guid}/documents/{documentId:guid}", async (
            Guid businessId,
            Guid documentId,
            HttpContext ctx,
            IBusinessAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteDocumentAsync(userId, documentId);
            return errorCode switch
            {
                "DOCUMENT_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Document not found.", "DOCUMENT_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this business.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteBusinessDocument")
        .WithSummary("Delete a document from a business (owner only)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}