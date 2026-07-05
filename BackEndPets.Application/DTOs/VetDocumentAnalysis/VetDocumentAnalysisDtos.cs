namespace BackEndPets.Application.DTOs.VetDocumentAnalysis;

public sealed record PetContextDto(
    string Name,
    string Species,
    string? Breed,
    int? Age,
    string? Sex,
    double? WeightKg);

public sealed record AnalyzeVetDocumentRequest(
    PetContextDto PetContext,
    IReadOnlyList<string> FileUrls,
    string? DocumentType,
    string? Symptoms);

public sealed record AbnormalValueDto(
    string Label,
    string Value,
    string? ReferenceRange,
    string Interpretation);

public sealed record VetDocumentAnalysisResponse(
    string Summary,
    IReadOnlyList<string> KeyFindings,
    IReadOnlyList<AbnormalValueDto> AbnormalValues,
    IReadOnlyList<string> PossibleConcerns,
    string UrgencyLevel,
    IReadOnlyList<string> QuestionsForVet,
    IReadOnlyList<string> MissingInformation,
    string Disclaimer);
