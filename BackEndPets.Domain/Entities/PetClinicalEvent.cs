namespace BackEndPets.Domain.Entities;

public static class PetClinicalEventType
{
    public const string Consultation = "consultation";
    public const string Vaccine = "vaccine";
    public const string Deworming = "deworming";
    public const string Surgery = "surgery";
    public const string Lab = "lab";
    public const string Hospitalization = "hospitalization";
    public const string Sterilization = "sterilization";
    public const string Other = "other";
}

public static class PetClinicalEventSource
{
    public const string Manual = "manual";
    public const string AiExtracted = "ai_extracted";
}

public sealed class PetClinicalEvent
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string EventType { get; set; } = PetClinicalEventType.Consultation;
    public DateTime EventDate { get; set; }
    public string? ClinicName { get; set; }
    public string? VeterinarianName { get; set; }
    public string? Reason { get; set; }
    public string? ClinicalSigns { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? Weight { get; set; }
    public string? BodyCondition { get; set; }
    public string? Findings { get; set; }
    public string? Diagnosis { get; set; }
    public string? ExamsPerformed { get; set; }
    public string? ExamResults { get; set; }
    public string? Procedures { get; set; }
    public string? Recommendations { get; set; }
    public string? Observations { get; set; }
    public DateTime? NextControlDate { get; set; }
    public string Source { get; set; } = PetClinicalEventSource.Manual;
    public DateTime CreatedAt { get; set; }

    public List<PetClinicalMedication> Medications { get; set; } = [];
}
