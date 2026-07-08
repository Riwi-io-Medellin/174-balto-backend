using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

// NOTE: vision reads images via image_url. Scanned PDFs must be rasterized to
// image URLs before calling this service (same limitation as
// OpenAIDocumentVerificationService). Word/Excel files are not supported here;
// UploadEndpoints should reject those for this flow or convert them upstream.
public sealed class OpenAIPetClinicalExtractionService(
    IConfiguration configuration,
    ILogger<OpenAIPetClinicalExtractionService> logger) : IPetClinicalExtractionService
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

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

        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("OpenAI:ApiKey is not configured.");
            return (null, "AI_NOT_CONFIGURED");
        }

        var configuredModel = configuration["OpenAI:Model"];
        var model = string.IsNullOrWhiteSpace(configuredModel) ? "gpt-4o-mini" : configuredModel;

        var userContent = new List<object> { new { type = "text", text = "Extract the clinical data from these documents." } };
        userContent.AddRange(fileUrls.Select(url => (object)new { type = "image_url", image_url = new { url } }));

        var requestBody = new
        {
            model,
            response_format = new { type = "json_object" },
            max_tokens = 2000,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = userContent }
            }
        };

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        HttpResponseMessage httpResponse;
        try
        {
            var json = JsonSerializer.Serialize(requestBody, JsonOpts);
            httpResponse = await http.PostAsync(ApiUrl, new StringContent(json, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OpenAI API call failed.");
            return (null, "AI_UNAVAILABLE");
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync();
            logger.LogError("OpenAI returned {Status}: {Body}", httpResponse.StatusCode, errorBody);
            return (null, "AI_UNAVAILABLE");
        }

        try
        {
            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                return (null, "AI_EMPTY_RESPONSE");

            var draft = JsonSerializer.Deserialize<ClinicalExtractionDraftResponse>(content, JsonOpts);
            if (draft is null)
                return (null, "AI_EXTRACTION_FAILED");

            return (draft, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse OpenAI response.");
            return (null, "AI_PARSE_ERROR");
        }
    }
}
