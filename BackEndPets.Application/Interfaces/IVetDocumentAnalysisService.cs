using BackEndPets.Application.DTOs.VetDocumentAnalysis;

namespace BackEndPets.Application.Interfaces;

public interface IVetDocumentAnalysisService
{
    Task<(VetDocumentAnalysisResponse? Result, string? ErrorCode)> AnalyzeAsync(
        Guid userId, AnalyzeVetDocumentRequest request, CancellationToken ct);

    Task<(IReadOnlyList<VetDocumentAnalysisHistoryItem>? Result, string? ErrorCode)> GetHistoryAsync(
        Guid userId, Guid petId, CancellationToken ct);
}
