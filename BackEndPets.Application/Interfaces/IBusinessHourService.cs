using BackEndPets.Application.DTOs.Businesses;

namespace BackEndPets.Application.Interfaces;

public interface IBusinessHourService
{
    Task<(IReadOnlyCollection<BusinessHourResponse>? Result, string? ErrorCode)> GetMyHoursAsync(Guid userId);

    Task<(IReadOnlyCollection<BusinessHourResponse>? Result, string? ErrorCode)> ReplaceMyHoursAsync(
        Guid userId, IEnumerable<BusinessHourRequest> hours);

    Task<(IReadOnlyCollection<BusinessHourExceptionResponse>? Result, string? ErrorCode)> GetMyExceptionsAsync(Guid userId);

    Task<(IReadOnlyCollection<BusinessHourExceptionResponse>? Result, string? ErrorCode)> ReplaceMyExceptionsAsync(
        Guid userId, IEnumerable<BusinessHourExceptionRequest> exceptions);
}