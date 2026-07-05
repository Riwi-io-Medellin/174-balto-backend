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

                return errorCode switch
                {
                    "VALIDATION_FAILED"   => Results.BadRequest(new ApiErrorResponse(
                        "Pet name, species, and at least one document are required.", "VALIDATION_FAILED")),
                    "AI_NOT_CONFIGURED"   => Results.StatusCode(503),
                    "AI_UNAVAILABLE"      => Results.StatusCode(503),
                    "AI_PARSE_ERROR"      => Results.StatusCode(502),
                    _                     => ResultsExtensions.UnhandledError()
                };
            })
            .WithTags("VetDocumentAnalysis")
            .WithName("AnalyzeVetDocument")
            .WithSummary("Analyze a veterinary document and return preliminary, non-diagnostic guidance")
            .RequireAuthorization()
            .Produces<VetDocumentAnalysisResponse>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status502BadGateway)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
