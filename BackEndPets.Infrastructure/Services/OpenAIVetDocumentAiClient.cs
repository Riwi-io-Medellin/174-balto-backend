using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class OpenAIVetDocumentAiClient(
    IConfiguration configuration,
    ILogger<OpenAIVetDocumentAiClient> logger) : IVetDocumentAiClient
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public string ProviderName => "openai";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(configuration["OpenAI:ApiKey"]);

    public async Task<string> GenerateAnalysisJsonAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<(byte[] Bytes, string MimeType)> attachments,
        CancellationToken ct)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

        var configuredModel = configuration["OpenAI:Model"];
        var model = string.IsNullOrWhiteSpace(configuredModel) ? "gpt-4o-mini" : configuredModel;

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
            logger.LogError("OpenAI API call failed: {ExceptionType}", ex.GetType().Name);
            throw;
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            logger.LogError("OpenAI returned HTTP {Status}", httpResponse.StatusCode);
            throw new InvalidOperationException($"OpenAI API returned {httpResponse.StatusCode}.");
        }

        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("OpenAI returned an empty response.");

        return text;
    }
}
