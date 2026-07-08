using System.Text;
using System.Text.Json;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

// Reemplaza a OpenAIPetClinicalExtractionService: misma interfaz y mismo
// SystemPrompt/JSON shape, pero usa Gemini (inlineData base64) en vez de
// OpenAI (image_url). Reusa IVetDocumentAttachmentFetcher ya existente
// para descargar los fileUrls como bytes.
public sealed class GeminiPetClinicalExtractionService(
    IConfiguration configuration,
    IVetDocumentAttachmentFetcher attachmentFetcher,
    ILogger<GeminiPetClinicalExtractionService> logger) : IPetClinicalExtractionService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string SystemPrompt = """
        You are a veterinary clinical-record extraction assistant.
        Extract every piece of medical information visible in the provided document images
        into the exact JSON shape requested below. Use null for any field not present.
        Dates must be ISO 8601. Do not invent data that is not in the documents.

        Respond ONLY with a JSON object with exactly this shape:
        {
          "petProfile": { "sex": null, "color": null, "identificationNumber": null, "microchipNumber": null, "weight": null },
          "clinicalRecord": { "allergies": null, "chronicConditions": null, "dietaryRestrictions": null },
          "event": {
            "eventType": "consultation",
            "eventDate": null,
            "clinicName": null, "veterinarianName": null, "reason": null,
            "clinicalSigns": null, "temperature": null, "heartRate": null, "respiratoryRate": null,
            "weight": null, "bodyCondition": null, "findings": null, "diagnosis": null,
            "examsPerformed": null, "examResults": null, "procedures": null,
            "recommendations": null, "observations": null, "nextControlDate": null,
            "medications": [ { "name": "", "dose": null, "frequency": null, "duration": null } ]
          }
        }
        eventType must be one of: consultation, vaccine, deworming, surgery, lab, hospitalization, sterilization, other.
        """;

    public async Task<(ClinicalExtractionDraftResponse? Draft, string? ErrorCode)> ExtractAsync(
        IReadOnlyCollection<string> fileUrls)
    {
        if (fileUrls.Count == 0)
            return (null, "NO_FILES");

        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Gemini:ApiKey is not configured.");
            return (null, "AI_NOT_CONFIGURED");
        }

        var configuredModel = configuration["Gemini:Model"];
        var model = string.IsNullOrWhiteSpace(configuredModel) ? "gemini-2.5-flash" : configuredModel;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments;
        try
        {
            attachments = await attachmentFetcher.FetchAsync(fileUrls.ToList(), CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download attachment(s) for clinical extraction.");
            return (null, "AI_UNAVAILABLE");
        }

        var parts = new List<object> { new { text = "Extract the clinical data from these documents." } };
        parts.AddRange(attachments.Select(a => (object)new
        {
            inlineData = new { mimeType = a.MimeType, data = Convert.ToBase64String(a.Bytes) }
        }));

        var requestBody = new
        {
            systemInstruction = new { parts = new object[] { new { text = SystemPrompt } } },
            contents = new object[] { new { role = "user", parts } },
            generationConfig = new { responseMimeType = "application/json" }
        };

        using var http = new HttpClient();

        HttpResponseMessage httpResponse;
        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            httpResponse = await http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini API call failed.");
            return (null, "AI_UNAVAILABLE");
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync();
            logger.LogError("Gemini returned {Status}: {Body}", httpResponse.StatusCode, errorBody);
            return (null, "AI_UNAVAILABLE");
        }

        try
        {
            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                var blockReason = doc.RootElement.TryGetProperty("promptFeedback", out var feedback)
                    ? feedback.ToString()
                    : "unknown";
                logger.LogError("Gemini returned no candidates (likely blocked): {BlockReason}", blockReason);
                return (null, "AI_EMPTY_RESPONSE");
            }

            var candidate = candidates[0];
            var finishReason = candidate.TryGetProperty("finishReason", out var fr) ? fr.GetString() : null;
            if (!candidate.TryGetProperty("content", out var contentElement) ||
                !contentElement.TryGetProperty("parts", out var partsElement) ||
                partsElement.GetArrayLength() == 0)
            {
                logger.LogError("Gemini candidate had no content parts (finishReason: {FinishReason})", finishReason);
                return (null, "AI_EMPTY_RESPONSE");
            }

            var content = partsElement[0].TryGetProperty("text", out var textElement) ? textElement.GetString() : null;

            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogError("Gemini returned empty text (finishReason: {FinishReason})", finishReason);
                return (null, "AI_EMPTY_RESPONSE");
            }

            var draft = JsonSerializer.Deserialize<ClinicalExtractionDraftResponse>(content, JsonOpts);
            if (draft is null)
                return (null, "AI_EXTRACTION_FAILED");

            return (draft, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse Gemini response.");
            return (null, "AI_PARSE_ERROR");
        }
    }
}
