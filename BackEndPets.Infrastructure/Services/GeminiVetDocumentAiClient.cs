using System.Text;
using System.Text.Json;
using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class GeminiVetDocumentAiClient(
    IConfiguration configuration,
    ILogger<GeminiVetDocumentAiClient> logger) : IVetDocumentAiClient
{
    public string ProviderName => "gemini";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(configuration["Gemini:ApiKey"]);

    public async Task<string> GenerateAnalysisJsonAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments,
        CancellationToken ct)
    {
        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini:ApiKey is not configured.");

        var configuredModel = configuration["Gemini:Model"];
        var model = string.IsNullOrWhiteSpace(configuredModel) ? "gemini-2.5-flash" : configuredModel;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var parts = new List<object> { new { text = userPrompt } };
        parts.AddRange(attachments.Select(a => (object)new
        {
            inlineData = new { mimeType = a.MimeType, data = Convert.ToBase64String(a.Bytes) }
        }));

        var requestBody = new
        {
            systemInstruction = new { parts = new object[] { new { text = systemPrompt } } },
            contents = new object[] { new { role = "user", parts } },
            generationConfig = new { responseMimeType = "application/json" }
        };

        using var http = new HttpClient();
        var json = JsonSerializer.Serialize(requestBody);

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);
        }
        catch (Exception ex)
        {
            logger.LogError("Gemini API call failed: {ExceptionType}", ex.GetType().Name);
            throw;
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            logger.LogError("Gemini returned HTTP {Status}", httpResponse.StatusCode);
            throw new InvalidOperationException($"Gemini API returned {httpResponse.StatusCode}.");
        }

        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.GetArrayLength() == 0)
        {
            var blockReason = doc.RootElement.TryGetProperty("promptFeedback", out var feedback)
                ? feedback.ToString()
                : "unknown";
            logger.LogError("Gemini returned no candidates (likely blocked): {BlockReason}", blockReason);
            throw new InvalidOperationException($"Gemini returned no candidates ({blockReason}).");
        }

        var candidate = candidates[0];
        var finishReason = candidate.TryGetProperty("finishReason", out var fr) ? fr.GetString() : null;
        if (!candidate.TryGetProperty("content", out var contentElement) ||
            !contentElement.TryGetProperty("parts", out var partsElement) ||
            partsElement.GetArrayLength() == 0)
        {
            logger.LogError("Gemini candidate had no content parts (finishReason: {FinishReason})", finishReason);
            throw new InvalidOperationException($"Gemini returned no content (finishReason: {finishReason}).");
        }

        var text = partsElement[0].TryGetProperty("text", out var textElement) ? textElement.GetString() : null;

        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogError("Gemini returned empty text (finishReason: {FinishReason})", finishReason);
            throw new InvalidOperationException($"Gemini returned an empty response (finishReason: {finishReason}).");
        }

        return text;
    }
}
