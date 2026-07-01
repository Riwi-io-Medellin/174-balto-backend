namespace BackEndPets.Application.DTOs.HomeServices;

// ── Weekly availability ──────────────────────────────────────────────────────

public sealed record HomeProviderAvailabilitySlotRequest(
    int DayOfWeek,        // 0 = Sunday … 6 = Saturday
    TimeOnly StartTime,
    TimeOnly EndTime);

public sealed record HomeProviderAvailabilitySlotResponse(
    Guid Id,
    Guid ProviderId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

// ── Date exceptions ──────────────────────────────────────────────────────────

public sealed record HomeProviderAvailabilityExceptionRequest(
    DateOnly Date,
    bool IsUnavailable,
    TimeOnly? StartTime,  // required when IsUnavailable = false
    TimeOnly? EndTime,    // required when IsUnavailable = false
    string? Reason = null);

public sealed record HomeProviderAvailabilityExceptionResponse(
    Guid Id,
    Guid ProviderId,
    DateOnly Date,
    bool IsUnavailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason);

// ── Available slots (computed) ───────────────────────────────────────────────

public sealed record HomeServiceAvailableSlotResponse(DateTime Start, DateTime End);
