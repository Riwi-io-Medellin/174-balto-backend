namespace BackEndPets.Application.Interfaces;

/// <summary>
/// A multimodal LLM client capable of analyzing veterinary document attachments
/// (images/PDFs) and returning a raw JSON string matching the caller's requested
/// shape. Implementations must not log attachment bytes, prompts, or responses.
/// </summary>
public interface IVetDocumentAiClient
{
    string ProviderName { get; }

    bool IsConfigured { get; }

    Task<string> GenerateAnalysisJsonAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments,
        CancellationToken ct);
}
