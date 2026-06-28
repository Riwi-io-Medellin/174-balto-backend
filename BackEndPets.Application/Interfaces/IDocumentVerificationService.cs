namespace BackEndPets.Application.Interfaces;

public interface IDocumentVerificationService
{
    /// <summary>
    /// Sends the image at <paramref name="imageUrl"/> to GPT-4o-mini and extracts
    /// the full name and document number printed on the identity document.
    /// </summary>
    /// <returns>
    /// (<see cref="DocumentVerificationResult"/>, null) on success,
    /// or (null, errorCode) when the AI call fails or the response cannot be parsed.
    /// </returns>
    Task<(DocumentVerificationResult? Result, string? ErrorCode)> ExtractAsync(string imageUrl);
}

public sealed record DocumentVerificationResult(string FullName, string DocumentNumber);
