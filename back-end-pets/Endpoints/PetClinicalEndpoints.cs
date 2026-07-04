using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class PetClinicalEndpoints
{
    public static IEndpointRouteBuilder MapPetClinicalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets")
            .WithTags("Pets")
            .RequireAuthorization();

        // 1. Registrar metadata de un archivo ya subido vía /api/upload (fuente para IA, no la historia oficial).
        group.MapPost("/{petId:guid}/clinical-record/documents", async (
            Guid petId,
            RegisterClinicalDocumentRequest request,
            HttpContext ctx,
            IPetClinicalRecordService service) =>
        {
            if (!TryGetUserId(ctx, out var userId)) return Results.Unauthorized();

            var (documentId, errorCode) = await service.UploadSourceDocumentAsync(
                userId, petId, request.FileUrl, request.FileName, request.FileType);

            return errorCode switch
            {
                "PET_NOT_FOUND" => NotFound("Pet not found.", "PET_NOT_FOUND"),
                "UNAUTHORIZED" => Forbidden(),
                _ => Results.Created($"/api/pets/{petId}/clinical-record/documents/{documentId}",
                    new { documentId })
            };
        })
        .WithName("RegisterClinicalDocument")
        .WithSummary("Register an uploaded source document for AI extraction (owner only)");

        // 2. Ejecutar OCR + extracción sobre los documentos subidos -> formulario prellenado.
        group.MapPost("/{petId:guid}/clinical-record/documents/extract", async (
            Guid petId,
            ExtractClinicalDocumentsRequest request,
            HttpContext ctx,
            IPetClinicalRecordService service) =>
        {
            if (!TryGetUserId(ctx, out var userId)) return Results.Unauthorized();

            var (draft, errorCode) = await service.RunExtractionAsync(userId, petId, request.DocumentIds);
            return errorCode switch
            {
                "PET_NOT_FOUND" => NotFound("Pet not found.", "PET_NOT_FOUND"),
                "UNAUTHORIZED" => Forbidden(),
                "DOCUMENT_NOT_FOUND" => NotFound("Document not found.", "DOCUMENT_NOT_FOUND"),
                "AI_NOT_CONFIGURED" or "AI_UNAVAILABLE" or "AI_EMPTY_RESPONSE" or "AI_PARSE_ERROR" or "AI_EXTRACTION_FAILED" =>
                    Results.Json(new ApiErrorResponse("AI extraction failed.", errorCode), statusCode: StatusCodes.Status502BadGateway),
                _ => Results.Ok(draft)
            };
        })
        .WithName("ExtractClinicalDocuments")
        .WithSummary("Run OCR + AI extraction over uploaded documents, returns a prefilled form (owner only)");

        // 3. Confirmar el formulario revisado por el usuario -> guarda evento + regenera documento + tips.
        group.MapPost("/{petId:guid}/clinical-record/events", async (
            Guid petId,
            ConfirmClinicalEventRequest request,
            HttpContext ctx,
            IPetClinicalRecordService service) =>
        {
            if (!TryGetUserId(ctx, out var userId)) return Results.Unauthorized();

            var (clinicalEvent, errorCode) = await service.ConfirmEventAsync(userId, petId, request);
            return errorCode switch
            {
                "PET_NOT_FOUND" => NotFound("Pet not found.", "PET_NOT_FOUND"),
                "UNAUTHORIZED" => Forbidden(),
                _ when clinicalEvent is not null => Results.Created(
                    $"/api/pets/{petId}/clinical-record/events/{clinicalEvent.Id}", clinicalEvent),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ConfirmClinicalEvent")
        .WithSummary("Save a reviewed clinical event; only this call persists data (owner only)")
        .Produces<ClinicalEventResponse>(StatusCodes.Status201Created);

        // 4. Historia clínica estructurada completa (para UI y para el ChatService).
        group.MapGet("/{petId:guid}/clinical-record", async (
            Guid petId,
            HttpContext ctx,
            IPetClinicalRecordService service) =>
        {
            if (!TryGetUserId(ctx, out var userId)) return Results.Unauthorized();

            var (record, errorCode) = await service.GetRecordAsync(userId, petId);
            return errorCode switch
            {
                "PET_NOT_FOUND" => NotFound("Pet not found.", "PET_NOT_FOUND"),
                "UNAUTHORIZED" => Forbidden(),
                _ => Results.Ok(record)
            };
        })
        .WithName("GetClinicalRecord")
        .WithSummary("Get the pet's full structured clinical history (owner only)")
        .Produces<ClinicalRecordResponse>(StatusCodes.Status200OK);

        // 5. Tips generados automáticamente al último cambio de historia.
        group.MapGet("/{petId:guid}/clinical-record/tips", async (
            Guid petId,
            HttpContext ctx,
            IPetClinicalRecordService service) =>
        {
            if (!TryGetUserId(ctx, out var userId)) return Results.Unauthorized();

            var (tips, errorCode) = await service.GetTipsAsync(userId, petId);
            return errorCode switch
            {
                "PET_NOT_FOUND" => NotFound("Pet not found.", "PET_NOT_FOUND"),
                "UNAUTHORIZED" => Forbidden(),
                _ => Results.Ok(tips)
            };
        })
        .WithName("GetClinicalTips")
        .WithSummary("Get the pet's current AI-generated care tips (owner only)");

        return app;
    }

    private static bool TryGetUserId(HttpContext ctx, out Guid userId)
    {
        var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out userId);
    }

    private static IResult NotFound(string message, string code) =>
        Results.NotFound(new ApiErrorResponse(message, code));

    private static IResult Forbidden() =>
        Results.Json(new ApiErrorResponse("You do not own this pet.", "UNAUTHORIZED"),
            statusCode: StatusCodes.Status403Forbidden);
}

public sealed record RegisterClinicalDocumentRequest(string FileUrl, string FileName, string FileType);
public sealed record ExtractClinicalDocumentsRequest(IReadOnlyCollection<Guid> DocumentIds);
