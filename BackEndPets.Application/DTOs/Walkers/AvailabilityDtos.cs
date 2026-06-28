namespace BackEndPets.Application.DTOs.Walkers;

// ── Weekly availability ──────────────────────────────────────────────────────

public sealed record AvailabilitySlotRequest(
    int DayOfWeek,        // 0 = Sunday … 6 = Saturday
    TimeOnly StartTime,
    TimeOnly EndTime);

public sealed record AvailabilitySlotResponse(
    Guid Id,
    Guid WalkerId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

// ── Date exceptions ──────────────────────────────────────────────────────────

public sealed record AvailabilityExceptionRequest(
    DateOnly Date,
    bool IsUnavailable,
    TimeOnly? StartTime,  // required when IsUnavailable = false
    TimeOnly? EndTime);   // required when IsUnavailable = false

public sealed record AvailabilityExceptionResponse(
    Guid Id,
    Guid WalkerId,
    DateOnly Date,
    bool IsUnavailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime);

// ── Available slots (computed) ───────────────────────────────────────────────

public sealed record AvailableSlotResponse(DateTime Start, DateTime End);
