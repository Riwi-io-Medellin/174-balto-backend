using BackEndPets.Application.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HttpVetDocumentAttachmentFetcher : IVetDocumentAttachmentFetcher
{
    public async Task<IReadOnlyList<(byte[] Bytes, string MimeType)>> FetchAsync(
        IReadOnlyList<string> fileUrls, CancellationToken ct)
    {
        using var http = new HttpClient();
        var results = new List<(byte[] Bytes, string MimeType)>(fileUrls.Count);
        foreach (var url in fileUrls)
        {
            var bytes = await http.GetByteArrayAsync(url, ct);
            results.Add((bytes, InferMimeType(url)));
        }
        return results;
    }

    private static string InferMimeType(string url)
    {
        var extension = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
