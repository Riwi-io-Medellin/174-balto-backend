using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.VetDocumentAnalysis;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class VetDocumentAnalysisEndpoints
{
    public static IEndpointRouteBuilder MapVetDocumentAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/vet-document-analysis", async (
                AnalyzeVetDocumentRequest request,
                HttpContext ctx,
                IVetDocumentAnalysisService service,
                CancellationToken ct) =>
            {
                var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var (result, errorCode) = await service.AnalyzeAsync(userId, request, ct);
                if (result is not null) return Results.Ok(result);

                // Some codes carry a "|<detail>" suffix (exception type name only,
                // never a message/body) to help diagnose without server log access.
                var parts = errorCode?.Split('|', 2);
                var baseCode = parts?[0];
                var detail = parts is { Length: 2 } ? parts[1] : null;

                return baseCode switch
                {
                    "VALIDATION_FAILED"          => Results.BadRequest(new ApiErrorResponse(
                        "Pet name, species, and at least one document are required.", "VALIDATION_FAILED")),
                    "PET_NOT_FOUND"              => Results.NotFound(new ApiErrorResponse(
                        "Pet not found.", "PET_NOT_FOUND")),
                    "AI_NOT_CONFIGURED"          => Results.Json(
                        new ApiErrorResponse(
                            "No AI provider is configured on the server (missing or invalid API key).",
                            "AI_NOT_CONFIGURED"),
                        statusCode: StatusCodes.Status503ServiceUnavailable),
                    "ATTACHMENT_FETCH_FAILED"    => Results.Json(
                        new ApiErrorResponse(
                            $"Could not download the uploaded document(s) for analysis. ({detail})",
                            "ATTACHMENT_FETCH_FAILED"),
                        statusCode: StatusCodes.Status503ServiceUnavailable),
                    "AI_UNAVAILABLE"             => Results.Json(
                        new ApiErrorResponse(
                            $"The AI provider could not be reached or returned an error. ({detail}) Please try again shortly.",
                            "AI_UNAVAILABLE"),
                        statusCode: StatusCodes.Status503ServiceUnavailable),
                    "AI_PARSE_ERROR"             => Results.Json(
                        new ApiErrorResponse(
                            "The AI response could not be understood. Please try again.",
                            "AI_PARSE_ERROR"),
                        statusCode: StatusCodes.Status502BadGateway),
                    _                            => ResultsExtensions.UnhandledError()
                };
            })
            .WithTags("VetDocumentAnalysis")
            .WithName("AnalyzeVetDocument")
            .WithSummary("Analyze a veterinary document and return preliminary, non-diagnostic guidance")
            .RequireAuthorization()
            .Produces<VetDocumentAnalysisResponse>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status502BadGateway)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/api/vet-document-analysis/pet/{petId:guid}/history", async (
                Guid petId,
                HttpContext ctx,
                IVetDocumentAnalysisService service,
                CancellationToken ct) =>
            {
                var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var (result, errorCode) = await service.GetHistoryAsync(userId, petId, ct);
                if (result is not null) return Results.Ok(result);

                return errorCode switch
                {
                    "PET_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND")),
                    _               => ResultsExtensions.UnhandledError()
                };
            })
            .WithTags("VetDocumentAnalysis")
            .WithName("GetVetDocumentAnalysisHistory")
            .WithSummary("Get the vet document analysis history for a pet")
            .RequireAuthorization()
            .Produces<IReadOnlyList<VetDocumentAnalysisHistoryItem>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
