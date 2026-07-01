using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class HomeProviderAssetsEndpoints
{
    public static IEndpointRouteBuilder MapHomeProviderAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home-services/providers")
            .WithTags("HomeServices")
            .RequireAuthorization();

        // Gallery
        group.MapPost("/me/gallery", async (
            AddHomeProviderPhotoRequest request,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (photo, errorCode) = await service.AddPhotoAsync(userId, request);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                _ when photo is not null => Results.Created($"/api/home-services/providers/me/gallery/{photo.Id}", photo),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddHomeProviderPhoto")
        .WithSummary("Add a photo to the current provider's gallery")
        .Produces<HomeProviderGalleryResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{providerId:guid}/gallery", async (Guid providerId, IHomeProviderAssetsService service) =>
            Results.Ok(await service.GetGalleryAsync(providerId)))
        .WithName("GetHomeProviderGallery")
        .WithSummary("Get all photos for a home service provider")
        .Produces<IReadOnlyCollection<HomeProviderGalleryResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/me/gallery/{photoId:guid}", async (
            Guid photoId,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeletePhotoAsync(userId, photoId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                "PHOTO_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Photo not found.", "PHOTO_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this photo.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteHomeProviderPhoto")
        .WithSummary("Delete a photo from the current provider's gallery")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // Documents
        group.MapPost("/me/documents", async (
            AddHomeProviderDocumentRequest request,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (document, errorCode) = await service.AddDocumentAsync(userId, request);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                _ when document is not null => Results.Created($"/api/home-services/providers/me/documents/{document.Id}", document),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddHomeProviderDocument")
        .WithSummary("Add a document to the current provider's profile")
        .Produces<HomeProviderDocumentResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{providerId:guid}/documents", async (Guid providerId, IHomeProviderAssetsService service) =>
            Results.Ok(await service.GetDocumentsAsync(providerId)))
        .WithName("GetHomeProviderDocuments")
        .WithSummary("Get all documents for a home service provider")
        .Produces<IReadOnlyCollection<HomeProviderDocumentResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/me/documents/{documentId:guid}", async (
            Guid documentId,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteDocumentAsync(userId, documentId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                "DOCUMENT_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Document not found.", "DOCUMENT_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this document.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteHomeProviderDocument")
        .WithSummary("Delete a document from the current provider's profile")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // Certifications
        group.MapPost("/me/certifications", async (
            AddHomeProviderCertificationRequest request,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (certification, errorCode) = await service.AddCertificationAsync(userId, request);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                _ when certification is not null => Results.Created(
                    $"/api/home-services/providers/me/certifications/{certification.Id}", certification),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddHomeProviderCertification")
        .WithSummary("Add a professional certification to the current provider's profile")
        .Produces<HomeProviderCertificationResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{providerId:guid}/certifications", async (Guid providerId, IHomeProviderAssetsService service) =>
            Results.Ok(await service.GetCertificationsAsync(providerId)))
        .WithName("GetHomeProviderCertifications")
        .WithSummary("Get all certifications for a home service provider")
        .Produces<IReadOnlyCollection<HomeProviderCertificationResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/me/certifications/{certificationId:guid}", async (
            Guid certificationId,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteCertificationAsync(userId, certificationId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                "CERTIFICATION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Certification not found.", "CERTIFICATION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this certification.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteHomeProviderCertification")
        .WithSummary("Delete a certification from the current provider's profile")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // Specialties
        group.MapPost("/me/specialties", async (
            AddHomeProviderSpecialtyRequest request,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (specialty, errorCode) = await service.AddSpecialtyAsync(userId, request);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                "SPECIALTY_ALREADY_EXISTS" => Results.Conflict(
                    new ApiErrorResponse("You already have this specialty listed.", "SPECIALTY_ALREADY_EXISTS")),
                _ when specialty is not null => Results.Created(
                    $"/api/home-services/providers/me/specialties/{specialty.Id}", specialty),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("AddHomeProviderSpecialty")
        .WithSummary("Add a specialty tag to the current provider's profile")
        .Produces<HomeProviderSpecialtyResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/{providerId:guid}/specialties", async (Guid providerId, IHomeProviderAssetsService service) =>
            Results.Ok(await service.GetSpecialtiesAsync(providerId)))
        .WithName("GetHomeProviderSpecialties")
        .WithSummary("Get all specialty tags for a home service provider")
        .Produces<IReadOnlyCollection<HomeProviderSpecialtyResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/me/specialties/{specialtyId:guid}", async (
            Guid specialtyId,
            HttpContext ctx,
            IHomeProviderAssetsService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (success, errorCode) = await service.DeleteSpecialtyAsync(userId, specialtyId);
            return errorCode switch
            {
                "PROVIDER_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("You do not have a home service provider profile.", "PROVIDER_NOT_FOUND")),
                "SPECIALTY_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Specialty not found.", "SPECIALTY_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("You do not own this specialty.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.NoContent()
            };
        })
        .WithName("DeleteHomeProviderSpecialty")
        .WithSummary("Delete a specialty tag from the current provider's profile")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
