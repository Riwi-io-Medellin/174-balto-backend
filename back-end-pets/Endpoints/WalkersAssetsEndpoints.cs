using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class WalkerAssetsEndpoints
{
    public static IEndpointRouteBuilder MapWalkerAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/walkers")
            .WithTags("Walkers")
            .RequireAuthorization();

        // Gallery
        group.MapPost("/me/gallery", async (
            AddWalkerPhotoRequest request,
            HttpContext ctx,
            IWalkerAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (photo, errorCode) = await service.AddPhotoAsync(userId, request);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a walker profile.", "WALKER_NOT_FOUND")),
                _ when photo is not null => Results.Created($"/api/walkers/me/gallery/{photo.Id}", photo),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddWalkerPhoto")
        .WithSummary("Add a photo to the current walker's gallery")
        .Produces<WalkerGalleryResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{walkerId:guid}/gallery", async (Guid walkerId, IWalkerAssetsService service) =>
            Results.Ok(await service.GetGalleryAsync(walkerId)))
        .WithName("GetWalkerGallery")
        .WithSummary("Get all photos for a walker")
        .Produces<IReadOnlyCollection<WalkerGalleryResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/me/gallery/{photoId:guid}", async (
            Guid photoId,
            HttpContext ctx,
            IWalkerAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeletePhotoAsync(userId, photoId);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a walker profile.", "WALKER_NOT_FOUND")),
                "PHOTO_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Photo not found.", "PHOTO_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this photo.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteWalkerPhoto")
        .WithSummary("Delete a photo from the current walker's gallery")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // Documents
        group.MapPost("/me/documents", async (
            AddWalkerDocumentRequest request,
            HttpContext ctx,
            IWalkerAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (document, errorCode) = await service.AddDocumentAsync(userId, request);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a walker profile.", "WALKER_NOT_FOUND")),
                _ when document is not null => Results.Created($"/api/walkers/me/documents/{document.Id}", document),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddWalkerDocument")
        .WithSummary("Add a document to the current walker's profile")
        .Produces<WalkerDocumentResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{walkerId:guid}/documents", async (Guid walkerId, IWalkerAssetsService service) =>
            Results.Ok(await service.GetDocumentsAsync(walkerId)))
        .WithName("GetWalkerDocuments")
        .WithSummary("Get all documents for a walker")
        .Produces<IReadOnlyCollection<WalkerDocumentResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/me/documents/{documentId:guid}", async (
            Guid documentId,
            HttpContext ctx,
            IWalkerAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteDocumentAsync(userId, documentId);
            return errorCode switch
            {
                "WALKER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a walker profile.", "WALKER_NOT_FOUND")),
                "DOCUMENT_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Document not found.", "DOCUMENT_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this document.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteWalkerDocument")
        .WithSummary("Delete a document from the current walker's profile")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}