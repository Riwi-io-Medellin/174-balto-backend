using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class OpenAIDocumentVerificationService(
    IConfiguration configuration,
    ILogger<OpenAIDocumentVerificationService> logger) : IDocumentVerificationService
{
    private const string Model = "gpt-4o-mini";
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<(DocumentVerificationResult? Result, string? ErrorCode)> ExtractAsync(string imageUrl)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("OpenAI:ApiKey is not configured.");
            return (null, "AI_NOT_CONFIGURED");
        }

        var requestBody = new
        {
            model = Model,
            response_format = new { type = "json_object" },
            max_tokens = 200,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
                        You are an identity document OCR assistant.
                        Extract the full name and document number from the provided identity document image.
                        Respond ONLY with a JSON object using exactly these keys:
                        { "fullName": "<full name as printed>", "documentNumber": "<document number>" }
                        If a field is not readable, use an empty string. Never add extra keys or explanation.
                        """
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image_url", image_url = new { url = imageUrl } }
                    }
                }
            }
        };

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        HttpResponseMessage httpResponse;
        try
        {
            var json = JsonSerializer.Serialize(requestBody, JsonOpts);
            httpResponse = await http.PostAsync(
                ApiUrl,
                new StringContent(json, Encoding.UTF8, "application/json"));
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

        string responseJson;
        try
        {
            responseJson = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            // OpenAI wraps the model reply inside choices[0].message.content (a JSON string).
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                return (null, "AI_EMPTY_RESPONSE");

            using var inner = JsonDocument.Parse(content);
            var fullName       = inner.RootElement.GetProperty("fullName").GetString() ?? string.Empty;
            var documentNumber = inner.RootElement.GetProperty("documentNumber").GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(documentNumber))
                return (null, "AI_EXTRACTION_FAILED");

            return (new DocumentVerificationResult(fullName.Trim(), documentNumber.Trim()), null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse OpenAI response.");
            return (null, "AI_PARSE_ERROR");
        }
    }

    /// <summary>
    /// Removes diacritics, lowercases, and collapses whitespace.
    /// Used externally by WalkerApplicationService for name comparison.
    /// </summary>
    public static string NormalizeName(string input)
    {
        var decomposed = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var clean = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        return string.Join(' ', clean.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
