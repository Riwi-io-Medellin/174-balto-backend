namespace BackEndPets.Domain.Entities;

public sealed class PetClinicalRecord
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime? DocumentGeneratedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
