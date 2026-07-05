using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

// NOTE: attachments are sent as base64 data URIs in image_url parts. Image
// support is broad across OpenRouter vision models; PDF support depends on
// the underlying model the configured OpenRouter model proxies to.
public sealed class OpenRouterVetDocumentAiClient(
    IConfiguration configuration,
    ILogger<OpenRouterVetDocumentAiClient> logger) : IVetDocumentAiClient
{
    private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";

    public string ProviderName => "openrouter";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(configuration["OpenRouter:ApiKey"]);

    public async Task<string> GenerateAnalysisJsonAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments,
        CancellationToken ct)
    {
        var apiKey = configuration["OpenRouter:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenRouter:ApiKey is not configured.");

        var configuredModel = configuration["OpenRouter:Model"];
        var model = string.IsNullOrWhiteSpace(configuredModel) ? "openai/gpt-4o-mini" : configuredModel;

        var userContent = new List<object> { new { type = "text", text = userPrompt } };
        userContent.AddRange(attachments.Select(a => (object)new
        {
            type = "image_url",
            image_url = new { url = $"data:{a.MimeType};base64,{Convert.ToBase64String(a.Bytes)}" }
        }));

        var requestBody = new
        {
            model,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userContent }
            }
        };

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var json = JsonSerializer.Serialize(requestBody);

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await http.PostAsync(ApiUrl, new StringContent(json, Encoding.UTF8, "application/json"), ct);
        }
        catch (Exception ex)
        {
            logger.LogError("OpenRouter API call failed: {ExceptionType}", ex.GetType().Name);
            throw;
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            logger.LogError("OpenRouter returned HTTP {Status}", httpResponse.StatusCode);
            throw new InvalidOperationException($"OpenRouter API returned {httpResponse.StatusCode}.");
        }

        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("OpenRouter returned an empty response.");

        return text;
    }
}
