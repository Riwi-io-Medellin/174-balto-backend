using BackEndPets.Application.Interfaces;

namespace BackEndPets.Tests.Fakes;

public sealed class FakeVetDocumentAttachmentFetcher : IVetDocumentAttachmentFetcher
{
    public Task<IReadOnlyList<(byte[] Bytes, string MimeType)>> FetchAsync(
        IReadOnlyList<string> fileUrls, CancellationToken ct)
    {
        IReadOnlyList<(byte[] Bytes, string MimeType)> result =
            fileUrls.Select(_ => (Bytes: new byte[] { 1, 2, 3 }, MimeType: "image/jpeg")).ToList();
        return Task.FromResult(result);
    }
}
