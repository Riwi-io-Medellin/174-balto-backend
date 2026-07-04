namespace BackEndPets.Domain.Entities;

public static class PetClinicalTipCategory
{
    public const string Care = "care";
    public const string Feeding = "feeding";
    public const string Vaccination = "vaccination";
    public const string Alert = "alert";
    public const string General = "general";
}

public sealed class PetClinicalTip
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string Category { get; set; } = PetClinicalTipCategory.General;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
