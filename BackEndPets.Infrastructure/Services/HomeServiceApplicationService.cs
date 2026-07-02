using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceApplicationService(
    IHomeServiceProviderRepository providerRepository,
    IHomeProviderDocumentRepository documentRepository,
    IDocumentVerificationService verificationService,
    UserManager<ApplicationUser> userManager,
    Cloudinary? cloudinary) : IHomeServiceApplicationService
{
    // A name is considered matching when at least this fraction of the user's
    // name words appear in the extracted name (case- and accent-insensitive).
    private const double SimilarityThreshold = 0.5;

    public async Task<(HomeServiceApplyResponse? Result, string? ErrorCode)> ApplyAsync(
        Guid userId,
        Stream documentStream,
        string documentFileName,
        HomeServiceApplyRequest request)
    {
        var warnings = new List<string>();
        string? documentUrl = null;
        string? extractedName = null;
        string? extractedNumber = null;

        // 1. Try uploading the identity document image to Cloudinary.
        if (cloudinary is not null)
        {
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(documentFileName, documentStream),
                    Folder = "balto/home-service-provider-documents",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error is not null)
                {
                    warnings.Add("Document could not be uploaded to cloud storage.");
                }
                else
                {
                    documentUrl = uploadResult.SecureUrl.ToString();
                }
            }
            catch
            {
                warnings.Add("Document upload failed due to a service error.");
            }
        }
        else
        {
            warnings.Add("Document upload service is not configured.");
        }

        // 2. Try running OCR via GPT-4o-mini to extract name and document number.
        if (documentUrl is not null)
        {
            try
            {
                var (extracted, aiError) = await verificationService.ExtractAsync(documentUrl);
                if (extracted is null)
                {
                    warnings.Add($"Document verification unavailable: {aiError}");
                }
                else
                {
                    extractedName = extracted.FullName;
                    extractedNumber = extracted.DocumentNumber;
                }
            }
            catch
            {
                warnings.Add("Document verification failed due to a service error.");
            }
        }

        // 3. Fetch the authenticated user to compare names.
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return (null, "USER_NOT_FOUND");

        // 4. Determine verification status.
        string verificationStatus;
        if (extractedName is not null)
        {
            var userFullName = $"{user.FirstName} {user.LastName}";
            var similarity = ComputeNameSimilarity(userFullName, extractedName);
            verificationStatus = similarity >= SimilarityThreshold ? "approved" : "rejected";
        }
        else
        {
            // Cannot verify without OCR — auto-approve so the user can proceed.
            verificationStatus = "approved";
            if (warnings.Count == 0)
                warnings.Add("Document could not be verified. Application auto-approved.");
        }

        // 5. Upsert the HomeServiceProvider record with verification outcome.
        var provider = await providerRepository.GetByUserIdAsync(userId);
        if (provider is null)
        {
            provider = await providerRepository.CreateAsync(new HomeServiceProvider
            {
                UserId             = userId,
                BaseLocation       = request.BaseLocation.Trim(),
                Experience         = request.Experience.Trim(),
                Description        = request.Description?.Trim(),
                VerificationStatus = verificationStatus,
                DocumentName       = extractedName,
                DocumentNumber     = extractedNumber
            });
        }
        else
        {
            provider.BaseLocation       = request.BaseLocation.Trim();
            provider.Experience         = request.Experience.Trim();
            provider.Description        = request.Description?.Trim();
            provider.VerificationStatus = verificationStatus;
            provider.DocumentName       = extractedName;
            provider.DocumentNumber     = extractedNumber;
            await providerRepository.UpdateAsync(provider);
        }

        // 6. Persist the identity document record (if uploaded).
        if (documentUrl is not null)
        {
            await documentRepository.CreateAsync(new HomeProviderDocument
            {
                ProviderId   = provider.Id,
                DocumentType = "identity",
                FileUrl      = documentUrl
            });
        }

        // 7. Build message.
        var message = warnings.Count > 0
            ? string.Join(" ", warnings)
            : verificationStatus == "approved"
                ? "Identity verified. Home service provider application approved."
                : "Identity could not be verified. Application rejected — name on document does not match account.";

        return (new HomeServiceApplyResponse(
            provider.Id,
            provider.UserId,
            provider.VerificationStatus,
            documentUrl ?? string.Empty,
            provider.DocumentName,
            provider.DocumentNumber,
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
