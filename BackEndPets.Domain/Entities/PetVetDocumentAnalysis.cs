namespace BackEndPets.Domain.Entities;

public sealed class PetVetDocumentAnalysis
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string? DocumentType { get; set; }
    public string? Symptoms { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty;
    public string ResultJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
