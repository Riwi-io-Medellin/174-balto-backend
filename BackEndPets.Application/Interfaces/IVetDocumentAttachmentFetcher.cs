namespace BackEndPets.Application.Interfaces;

/// <summary>
/// Downloads previously-uploaded document URLs into raw bytes for handing off
/// to a multimodal AI client. Kept separate from IVetDocumentAiClient so the
/// analysis service can be unit-tested without making real HTTP requests.
/// </summary>
public interface IVetDocumentAttachmentFetcher
{
    Task<IReadOnlyList<(byte[] Bytes, string MimeType)>> FetchAsync(
        IReadOnlyList<string> fileUrls, CancellationToken ct);
}
