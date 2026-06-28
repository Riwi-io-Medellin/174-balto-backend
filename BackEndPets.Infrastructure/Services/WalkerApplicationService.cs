using BackEndPets.Application.DTOs.Walkers;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Services;

public sealed class WalkerApplicationService(
    IWalkerRepository walkerRepository,
    IWalkerDocumentRepository documentRepository,
    IDocumentVerificationService verificationService,
    UserManager<ApplicationUser> userManager,
    Cloudinary? cloudinary) : IWalkerApplicationService
{
    // A name is considered matching when at least this fraction of the user's
    // name words appear in the extracted name (case- and accent-insensitive).
    private const double SimilarityThreshold = 0.5;

    public async Task<(WalkerApplyResponse? Result, string? ErrorCode)> ApplyAsync(
        Guid userId,
        Stream documentStream,
        string documentFileName,
        WalkerApplyRequest request)
    {
        if (cloudinary is null)
            return (null, "CLOUDINARY_NOT_CONFIGURED");

        // 1. Upload the identity document image to Cloudinary.
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(documentFileName, documentStream),
            Folder = "balto/walker-documents",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error is not null)
            return (null, "UPLOAD_FAILED");

        var documentUrl = uploadResult.SecureUrl.ToString();

        // 2. Run OCR via GPT-4o-mini to extract name and document number.
        var (extracted, aiError) = await verificationService.ExtractAsync(documentUrl);
        if (extracted is null)
            return (null, aiError);

        // 3. Fetch the authenticated user to compare names.
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return (null, "USER_NOT_FOUND");

        var userFullName    = $"{user.FirstName} {user.LastName}";
        var similarity      = ComputeNameSimilarity(userFullName, extracted.FullName);
        var verificationStatus = similarity >= SimilarityThreshold ? "approved" : "rejected";

        // 4. Upsert the Walker record with verification outcome.
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null)
        {
            walker = await walkerRepository.CreateAsync(new Walker
            {
                UserId             = userId,
                WorkLocation       = request.WorkLocation.Trim(),
                Experience         = request.Experience.Trim(),
                Description        = request.Description?.Trim(),
                VerificationStatus = verificationStatus,
                DocumentName       = extracted.FullName,
                DocumentNumber     = extracted.DocumentNumber
            });
        }
        else
        {
            walker.WorkLocation       = request.WorkLocation.Trim();
            walker.Experience         = request.Experience.Trim();
            walker.Description        = request.Description?.Trim();
            walker.VerificationStatus = verificationStatus;
            walker.DocumentName       = extracted.FullName;
            walker.DocumentNumber     = extracted.DocumentNumber;
            await walkerRepository.UpdateAsync(walker);
        }

        // 5. Persist the identity document record.
        await documentRepository.CreateAsync(new WalkerDocument
        {
            WalkerId     = walker.Id,
            DocumentType = "identity",
            FileUrl      = documentUrl
        });

        var message = verificationStatus == "approved"
            ? "Identity verified. Walker application approved."
            : "Identity could not be verified. Application rejected — name on document does not match account.";

        return (new WalkerApplyResponse(
            walker.Id,
            walker.UserId,
            walker.VerificationStatus,
            documentUrl,
            walker.DocumentName,
            walker.DocumentNumber,
            message), null);
    }

    /// <summary>
    /// Word-overlap similarity: fraction of <paramref name="userName"/> words
    /// found anywhere in <paramref name="extractedName"/> after normalization.
    /// Threshold ≥ 0.5 → approved.
    /// </summary>
    private static double ComputeNameSimilarity(string userName, string extractedName)
    {
        var userWords    = OpenAIDocumentVerificationService.NormalizeName(userName)
                               .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var extractedNorm = OpenAIDocumentVerificationService.NormalizeName(extractedName);

        if (userWords.Length == 0) return 0d;

        var matches = userWords.Count(w => extractedNorm.Contains(w));
        return (double)matches / userWords.Length;
    }
}
