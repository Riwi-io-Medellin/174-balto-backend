using System.Text.Json;
using BackEndPets.Application.DTOs.VetDocumentAnalysis;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class VetDocumentAnalysisService(
    IEnumerable<IVetDocumentAiClient> clients,
    IVetDocumentAttachmentFetcher attachmentFetcher,
    IPetService petService,
    IPetClinicalRepository clinicalRepository,
    IConfiguration configuration,
    ILogger<VetDocumentAnalysisService> logger) : IVetDocumentAnalysisService
{
    public const string RequiredDisclaimer =
        "This is not a veterinary diagnosis, treatment plan, or substitute for professional veterinary care. Please consult a licensed veterinarian for medical decisions.";

    private static readonly string[] AllowedUrgencyLevels =
        ["routine", "schedule_vet_visit", "urgent", "emergency"];

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string SystemPrompt = """
        You are a veterinary document guidance assistant. Your role is to help a pet
        owner understand a veterinary document (blood test, ultrasound report, PCR/lab
        result, prescription, or similar) in plain language, BEFORE they speak to a
        licensed veterinarian.

        You must NEVER provide a diagnosis, a treatment plan, medication choices, or
        dosage advice. You provide preliminary, educational guidance only, and you
        must always recommend consulting a licensed veterinarian for any medical
        decision. If the document or context is insufficient, say so explicitly rather
        than guessing.

        Respond ONLY with a JSON object with exactly this shape:
        {
          "summary": string,
          "keyFindings": string[],
          "abnormalValues": [
            { "label": string, "value": string, "referenceRange": string | null, "interpretation": string }
          ],
          "possibleConcerns": string[],
          "urgencyLevel": "routine" | "schedule_vet_visit" | "urgent" | "emergency",
          "questionsForVet": string[],
          "missingInformation": string[],
          "disclaimer": string
        }
        """;

    public async Task<(IReadOnlyList<VetDocumentAnalysisHistoryItem>? Result, string? ErrorCode)> GetHistoryAsync(
        Guid userId, Guid petId, CancellationToken ct)
    {
        var pet = await petService.GetByIdAsync(petId);
        if (pet is null || pet.UserId != userId)
        {
            return (null, "PET_NOT_FOUND");
        }

        var records = await clinicalRepository.GetVetDocumentAnalysesByPetIdAsync(petId, take: 20);
        var items = records
            .Select(r =>
            {
                var result = JsonSerializer.Deserialize<VetDocumentAnalysisResponse>(r.ResultJson, JsonOpts);
                return result is null ? null : new VetDocumentAnalysisHistoryItem(r.Id, r.CreatedAt, r.DocumentType, result);
            })
            .Where(i => i is not null)
            .Select(i => i!)
            .ToList();

        return (items, null);
    }

    public async Task<(VetDocumentAnalysisResponse? Result, string? ErrorCode)> AnalyzeAsync(
        Guid userId, AnalyzeVetDocumentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PetContext.Name) ||
            string.IsNullOrWhiteSpace(request.PetContext.Species) ||
            request.FileUrls.Count == 0 ||
            request.FileUrls.Count > 5)
        {
            return (null, "VALIDATION_FAILED");
        }

        var pet = await petService.GetByIdAsync(request.PetId);
        if (pet is null || pet.UserId != userId)
        {
            return (null, "PET_NOT_FOUND");
        }

        var configuredProvider = configuration["Ai:VetDocumentProvider"];
        var providerName = string.IsNullOrWhiteSpace(configuredProvider) ? "gemini" : configuredProvider;
        var clientList = clients.ToList();
        var primary = clientList.FirstOrDefault(c => c.ProviderName == providerName);
        var secondary = clientList.FirstOrDefault(c => c.ProviderName != providerName);

        if ((primary is null || !primary.IsConfigured) && (secondary is null || !secondary.IsConfigured))
        {
            logger.LogWarning("No vet-document AI provider is configured.");
            return (null, "AI_NOT_CONFIGURED");
        }

        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments;
        try
        {
            attachments = await attachmentFetcher.FetchAsync(request.FileUrls, ct);
        }
        catch (Exception ex)
        {
            var detail = ex is HttpRequestException { StatusCode: not null } httpEx
                ? $"{ex.GetType().Name} {(int)httpEx.StatusCode}"
                : ex.GetType().Name;
            logger.LogError("Failed to download attachment(s) for analysis: {Detail}", detail);
            return (null, $"ATTACHMENT_FETCH_FAILED|{detail}");
        }

        var userPrompt = BuildUserPrompt(request);

        var (text, primaryError) = primary is null
            ? (null, null)
            : await TryGenerateAsync(primary, userPrompt, attachments, ct);
        var lastError = primaryError;
        if (text is null && secondary is not null)
        {
            (text, lastError) = await TryGenerateAsync(secondary, userPrompt, attachments, ct);
        }

        if (text is null)
            return (null, $"AI_UNAVAILABLE|{lastError ?? "no configured provider succeeded"}");

        try
        {
            var result = JsonSerializer.Deserialize<VetDocumentAnalysisResponse>(text, JsonOpts);
            if (result is null)
                return (null, "AI_PARSE_ERROR");

            var normalized = result with
            {
                UrgencyLevel = AllowedUrgencyLevels.Contains(result.UrgencyLevel)
                    ? result.UrgencyLevel
                    : "schedule_vet_visit",
                Disclaimer = RequiredDisclaimer
            };

            await PersistAsync(request, normalized);

            return (normalized, null);
        }
        catch (JsonException)
        {
            logger.LogError("Failed to parse AI response into the expected JSON shape.");
            return (null, "AI_PARSE_ERROR");
        }
    }

    private async Task PersistAsync(AnalyzeVetDocumentRequest request, VetDocumentAnalysisResponse response)
    {
        try
        {
            await clinicalRepository.CreateVetDocumentAnalysisAsync(new PetVetDocumentAnalysis
            {
                PetId = request.PetId,
                DocumentType = request.DocumentType,
                Symptoms = request.Symptoms,
                UrgencyLevel = response.UrgencyLevel,
                ResultJson = JsonSerializer.Serialize(response, JsonOpts)
            });
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to persist vet document analysis result: {ExceptionType}", ex.GetType().Name);
        }
    }

    private async Task<(string? Json, string? ErrorDetail)> TryGenerateAsync(
        IVetDocumentAiClient client,
        string userPrompt,
        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments,
        CancellationToken ct)
    {
        if (!client.IsConfigured) return (null, $"{client.ProviderName} not configured");
        try
        {
            var json = await client.GenerateAnalysisJsonAsync(SystemPrompt, userPrompt, attachments, ct);
            return (json, null);
        }
        catch (Exception ex)
        {
            logger.LogError("{Provider} analysis attempt failed: {ExceptionType}", client.ProviderName, ex.GetType().Name);
            return (null, $"{client.ProviderName}: {ex.GetType().Name}");
        }
    }

    private static string BuildUserPrompt(AnalyzeVetDocumentRequest request)
    {
        var ctx = request.PetContext;
        var lines = new List<string>
        {
            "Analyze the attached veterinary document(s) for this pet:",
            $"- Name: {ctx.Name}",
            $"- Species: {ctx.Species}",
        };
        if (!string.IsNullOrWhiteSpace(ctx.Breed)) lines.Add($"- Breed: {ctx.Breed}");
        if (ctx.Age is not null) lines.Add($"- Age: {ctx.Age} years");
        if (!string.IsNullOrWhiteSpace(ctx.Sex)) lines.Add($"- Sex: {ctx.Sex}");
        if (ctx.WeightKg is not null) lines.Add($"- Weight: {ctx.WeightKg} kg");
        if (!string.IsNullOrWhiteSpace(request.DocumentType)) lines.Add($"- Document type: {request.DocumentType}");
        if (!string.IsNullOrWhiteSpace(request.Symptoms)) lines.Add($"- Reported symptoms/reason: {request.Symptoms}");
        return string.Join('\n', lines);
    }
}
