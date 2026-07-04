using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class OpenAIPetClinicalTipsService(
    IConfiguration configuration,
    ILogger<OpenAIPetClinicalTipsService> logger) : IPetClinicalTipsService
{
    private const string Model = "gpt-4o-mini";
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    private sealed record TipDraft(string Category, string Message);
    private sealed record TipsWrapper(List<TipDraft> Tips);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IReadOnlyCollection<PetClinicalTip>> GenerateAsync(
        Pet pet, PetClinicalRecord record, IReadOnlyCollection<PetClinicalEvent> events)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("OpenAI:ApiKey is not configured; skipping tips generation.");
            return [];
        }

        var eventsSummary = events.Count == 0
            ? "No clinical events recorded yet."
            : string.Join("\n", events.OrderByDescending(e => e.EventDate).Take(10).Select(e =>
                $"- {e.EventDate:yyyy-MM-dd} {e.EventType}: reason={e.Reason}, diagnosis={e.Diagnosis}, next control={e.NextControlDate:yyyy-MM-dd}"));

        var prompt = $"""
            Pet: {pet.Name}, species={pet.Species}, breed={pet.Breed}, birth date={pet.BirthDate:yyyy-MM-dd}, weight={pet.Weight}kg.
            Allergies: {record.Allergies ?? "none"}. Chronic conditions: {record.ChronicConditions ?? "none"}. Diet restrictions: {record.DietaryRestrictions ?? "none"}.

            Recent clinical events:
            {eventsSummary}

            Generate 3 to 6 short, personalized tips for the owner based only on this data
            (care, feeding, pending controls, vaccination status, alerts, general advice).
            Respond ONLY with JSON: { "tips": [ { "category": "care|feeding|vaccination|alert|general", "message": "..." } ] }
            """;

        var requestBody = new
        {
            model = Model,
            response_format = new { type = "json_object" },
            max_tokens = 600,
            messages = new object[]
            {
                new { role = "system", content = "You are a veterinary assistant generating short personalized pet-care tips." },
                new { role = "user", content = prompt }
            }
        };

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            var json = JsonSerializer.Serialize(requestBody, JsonOpts);
            var httpResponse = await http.PostAsync(ApiUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (!httpResponse.IsSuccessStatusCode)
            {
                logger.LogError("OpenAI returned {Status} generating tips.", httpResponse.StatusCode);
                return [];
            }

            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            if (string.IsNullOrWhiteSpace(content))
                return [];

            var wrapper = JsonSerializer.Deserialize<TipsWrapper>(content, JsonOpts);
            if (wrapper is null) return [];

            return wrapper.Tips
                .Select(t => new PetClinicalTip { Category = t.Category, Message = t.Message })
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate clinical tips.");
            return [];
        }
    }
}
