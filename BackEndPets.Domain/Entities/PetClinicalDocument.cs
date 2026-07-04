namespace BackEndPets.Domain.Entities;

public static class PetClinicalDocumentStatus
{
    public const string Pending = "pending";
    public const string Processed = "processed";
    public const string Failed = "failed";
}

public sealed class PetClinicalDocument
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string Status { get; set; } = PetClinicalDocumentStatus.Pending;
    public string? ExtractedJson { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
