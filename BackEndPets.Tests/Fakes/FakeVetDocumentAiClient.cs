using BackEndPets.Application.Interfaces;

namespace BackEndPets.Tests.Fakes;

public sealed class FakeVetDocumentAiClient(string providerName) : IVetDocumentAiClient
{
    public string ProviderName => providerName;

    public bool IsConfigured { get; set; } = true;

    public string? ResponseJson { get; set; }

    public Exception? ThrowOnGenerate { get; set; }

    public Task<string> GenerateAnalysisJsonAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments,
        CancellationToken ct)
    {
        if (ThrowOnGenerate is not null) throw ThrowOnGenerate;
        return Task.FromResult(ResponseJson ?? "{}");
    }
}
