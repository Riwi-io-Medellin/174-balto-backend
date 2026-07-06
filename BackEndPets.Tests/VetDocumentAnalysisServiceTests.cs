using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.DTOs.VetDocumentAnalysis;
using BackEndPets.Infrastructure.Services;
using BackEndPets.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackEndPets.Tests;

public sealed class VetDocumentAnalysisServiceTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidPetId = Guid.NewGuid();

    private static PetResponse OwnedPet() => new(
        ValidPetId, ValidUserId, "Rocky", "dog", null, null, null, null, null, null, null, null, null,
        DateTime.UtcNow, false, null, null, null);

    private const string ValidResponseJson = """
        {
          "summary": "The blood panel looks mostly normal.",
          "keyFindings": ["Slightly elevated white cell count"],
          "abnormalValues": [
            { "label": "WBC", "value": "18.2", "referenceRange": "6-17", "interpretation": "Mildly elevated" }
          ],
          "possibleConcerns": ["Possible mild infection or stress response"],
          "urgencyLevel": "schedule_vet_visit",
          "questionsForVet": ["Could this be related to recent vaccination?"],
          "missingInformation": ["No prior baseline bloodwork for comparison"],
          "disclaimer": "some model-provided text we must not trust"
        }
        """;

    private static IConfiguration BuildConfig(string provider = "gemini") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Ai:VetDocumentProvider"] = provider })
            .Build();

    private static VetDocumentAnalysisService Build(
        FakeVetDocumentAiClient gemini, FakeVetDocumentAiClient openRouter, string provider = "gemini",
        FakePetService? petService = null, FakePetClinicalRepository? clinicalRepository = null) =>
        new(
            [gemini, openRouter],
            new FakeVetDocumentAttachmentFetcher(),
            petService ?? new FakePetService { PetToReturn = OwnedPet() },
            clinicalRepository ?? new FakePetClinicalRepository(),
            BuildConfig(provider),
            NullLogger<VetDocumentAnalysisService>.Instance);

    private static AnalyzeVetDocumentRequest ValidRequest(IReadOnlyList<string>? fileUrls = null) => new(
        ValidPetId,
        new PetContextDto("Rocky", "dog", null, null, null, null),
        fileUrls ?? ["https://cdn.example.com/report.jpg"],
        null,
        null);

    [Fact]
    public async Task AnalyzeAsync_MissingPetName_ReturnsValidationFailed()
    {
        var service = Build(new FakeVetDocumentAiClient("gemini"), new FakeVetDocumentAiClient("openrouter"));
        var request = ValidRequest() with { PetContext = new PetContextDto("", "dog", null, null, null, null) };

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, request, CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("VALIDATION_FAILED", errorCode);
    }

    [Fact]
    public async Task AnalyzeAsync_NoFiles_ReturnsValidationFailed()
    {
        var service = Build(new FakeVetDocumentAiClient("gemini"), new FakeVetDocumentAiClient("openrouter"));
        var request = ValidRequest(fileUrls: []);

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, request, CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("VALIDATION_FAILED", errorCode);
    }

    [Fact]
    public async Task AnalyzeAsync_SuccessfulResponse_ForcesRequiredDisclaimer()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { ResponseJson = ValidResponseJson };
        var service = Build(gemini, new FakeVetDocumentAiClient("openrouter"));

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(errorCode);
        Assert.NotNull(result);
        Assert.Equal(VetDocumentAnalysisService.RequiredDisclaimer, result!.Disclaimer);
        Assert.Equal("schedule_vet_visit", result.UrgencyLevel);
        Assert.Single(result.AbnormalValues);
    }

    [Fact]
    public async Task AnalyzeAsync_UnrecognizedUrgencyLevel_NormalizesToScheduleVetVisit()
    {
        const string json = """
            {
              "summary": "x", "keyFindings": [], "abnormalValues": [], "possibleConcerns": [],
              "urgencyLevel": "not_a_real_level", "questionsForVet": [], "missingInformation": [],
              "disclaimer": "x"
            }
            """;
        var gemini = new FakeVetDocumentAiClient("gemini") { ResponseJson = json };
        var service = Build(gemini, new FakeVetDocumentAiClient("openrouter"));

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(errorCode);
        Assert.Equal("schedule_vet_visit", result!.UrgencyLevel);
    }

    [Fact]
    public async Task AnalyzeAsync_MalformedJson_ReturnsAiParseError()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { ResponseJson = "not json at all" };
        var service = Build(gemini, new FakeVetDocumentAiClient("openrouter"));

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("AI_PARSE_ERROR", errorCode);
    }

    [Fact]
    public async Task AnalyzeAsync_PrimaryNotConfigured_FallsBackToSecondary()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { IsConfigured = false };
        var openRouter = new FakeVetDocumentAiClient("openrouter") { ResponseJson = ValidResponseJson };
        var service = Build(gemini, openRouter, provider: "gemini");

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(errorCode);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AnalyzeAsync_PrimaryThrows_FallsBackToSecondary()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { ThrowOnGenerate = new InvalidOperationException("boom") };
        var openRouter = new FakeVetDocumentAiClient("openrouter") { ResponseJson = ValidResponseJson };
        var service = Build(gemini, openRouter, provider: "gemini");

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(errorCode);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AnalyzeAsync_NoProviderConfigured_ReturnsAiNotConfigured()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { IsConfigured = false };
        var openRouter = new FakeVetDocumentAiClient("openrouter") { IsConfigured = false };
        var service = Build(gemini, openRouter);

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("AI_NOT_CONFIGURED", errorCode);
    }

    [Fact]
    public async Task AnalyzeAsync_PetNotOwnedByUser_ReturnsPetNotFound()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { ResponseJson = ValidResponseJson };
        var petService = new FakePetService { PetToReturn = OwnedPet() with { UserId = Guid.NewGuid() } };
        var service = Build(gemini, new FakeVetDocumentAiClient("openrouter"), petService: petService);

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("PET_NOT_FOUND", errorCode);
    }

    [Fact]
    public async Task AnalyzeAsync_PetDoesNotExist_ReturnsPetNotFound()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { ResponseJson = ValidResponseJson };
        var petService = new FakePetService { PetToReturn = null };
        var service = Build(gemini, new FakeVetDocumentAiClient("openrouter"), petService: petService);

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("PET_NOT_FOUND", errorCode);
    }

    [Fact]
    public async Task AnalyzeAsync_SuccessfulResponse_PersistsAnalysisForPet()
    {
        var gemini = new FakeVetDocumentAiClient("gemini") { ResponseJson = ValidResponseJson };
        var clinicalRepository = new FakePetClinicalRepository();
        var service = Build(gemini, new FakeVetDocumentAiClient("openrouter"), clinicalRepository: clinicalRepository);

        var (result, errorCode) = await service.AnalyzeAsync(ValidUserId, ValidRequest(), CancellationToken.None);

        Assert.Null(errorCode);
        Assert.NotNull(result);
        var persisted = Assert.Single(clinicalRepository.CreatedAnalyses);
        Assert.Equal(ValidPetId, persisted.PetId);
        Assert.Equal("schedule_vet_visit", persisted.UrgencyLevel);
        Assert.Contains("blood panel", persisted.ResultJson);
    }
}
