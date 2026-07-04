namespace BackEndPets.Application.DTOs.Pets;

// ---- Upload: fuente para IA, no se guarda como historia oficial ----
public sealed record UploadClinicalDocumentsResponse(
    IReadOnlyCollection<Guid> DocumentIds);

// ---- Draft: lo que la IA extrae y el usuario revisa/edita antes de Guardar ----
public sealed record ClinicalMedicationDraft(
    string Name,
    string? Dose,
    string? Frequency,
    string? Duration);

public sealed record ClinicalEventDraft(
    string EventType,
    DateTime EventDate,
    string? ClinicName,
    string? VeterinarianName,
    string? Reason,
    string? ClinicalSigns,
    decimal? Temperature,
    int? HeartRate,
    int? RespiratoryRate,
    decimal? Weight,
    string? BodyCondition,
    string? Findings,
    string? Diagnosis,
    string? ExamsPerformed,
    string? ExamResults,
    string? Procedures,
    string? Recommendations,
    string? Observations,
    DateTime? NextControlDate,
    IReadOnlyCollection<ClinicalMedicationDraft> Medications);

public sealed record PetProfileDraft(
    string? Sex,
    string? Color,
    string? IdentificationNumber,
    string? MicrochipNumber,
    decimal? Weight);

public sealed record ClinicalRecordDraft(
    string? Allergies,
    string? ChronicConditions,
    string? DietaryRestrictions);

// Formulario completo prellenado que recibe el frontend (mismo shape para editar y para confirmar)
public sealed record ClinicalExtractionDraftResponse(
    PetProfileDraft PetProfile,
    ClinicalRecordDraft ClinicalRecord,
    ClinicalEventDraft Event);

// ---- Confirmación del usuario: guarda exactamente lo mismo que revisó/editó ----
public sealed record ConfirmClinicalEventRequest(
    PetProfileDraft PetProfile,
    ClinicalRecordDraft ClinicalRecord,
    ClinicalEventDraft Event);

// ---- Lectura ----
public sealed record ClinicalMedicationResponse(
    Guid Id, string Name, string? Dose, string? Frequency, string? Duration);

public sealed record ClinicalEventResponse(
    Guid Id,
    Guid PetId,
    string EventType,
    DateTime EventDate,
    string? ClinicName,
    string? VeterinarianName,
    string? Reason,
    string? ClinicalSigns,
    decimal? Temperature,
    int? HeartRate,
    int? RespiratoryRate,
    decimal? Weight,
    string? BodyCondition,
    string? Findings,
    string? Diagnosis,
    string? ExamsPerformed,
    string? ExamResults,
    string? Procedures,
    string? Recommendations,
    string? Observations,
    DateTime? NextControlDate,
    string Source,
    DateTime CreatedAt,
    IReadOnlyCollection<ClinicalMedicationResponse> Medications);

public sealed record ClinicalRecordResponse(
    Guid PetId,
    string? Allergies,
    string? ChronicConditions,
    string? DietaryRestrictions,
    string? DocumentUrl,
    DateTime? DocumentGeneratedAt,
    IReadOnlyCollection<ClinicalEventResponse> Events);

public sealed record ClinicalTipResponse(
    Guid Id, string Category, string Message, DateTime CreatedAt);
