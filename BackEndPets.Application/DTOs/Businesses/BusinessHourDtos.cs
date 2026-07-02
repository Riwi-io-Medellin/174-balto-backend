namespace BackEndPets.Application.DTOs.Businesses;

public sealed record BusinessHourRequest(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public sealed record BusinessHourResponse(
    Guid Id,
    Guid BusinessId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public sealed record BusinessHourExceptionRequest(
    DateOnly Date,
    bool IsUnavailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime);

public sealed record BusinessHourExceptionResponse(
    Guid Id,
    Guid BusinessId,
    DateOnly Date,
    bool IsUnavailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime);