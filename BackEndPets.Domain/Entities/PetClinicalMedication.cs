namespace BackEndPets.Domain.Entities;

public sealed class PetClinicalMedication
{
    public Guid Id { get; set; }
    public Guid ClinicalEventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Dose { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}
