using BackEndPets.Application.DTOs.Businesses;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class BusinessHourService(
    IBusinessRepository businessRepository,
    IBusinessHourRepository hourRepository,
    IBusinessHourExceptionRepository exceptionRepository) : IBusinessHourService
{
    public async Task<(IReadOnlyCollection<BusinessHourResponse>? Result, string? ErrorCode)> GetMyHoursAsync(Guid userId)
    {
        var business = await GetMyBusinessAsync(userId);
        if (business is null) return (null, "BUSINESS_NOT_FOUND");

        return ((await hourRepository.GetByBusinessIdAsync(business.Id)).Select(MapHour).ToList(), null);
    }

    public async Task<(IReadOnlyCollection<BusinessHourResponse>? Result, string? ErrorCode)> ReplaceMyHoursAsync(
        Guid userId, IEnumerable<BusinessHourRequest> hours)
    {
        var business = await GetMyBusinessAsync(userId);
        if (business is null) return (null, "BUSINESS_NOT_FOUND");
        if (business.VerificationStatus != "approved") return (null, "BUSINESS_NOT_APPROVED");

        var list = hours.ToList();
        foreach (var h in list)
        {
            if (h.DayOfWeek is < 0 or > 6) return (null, "INVALID_DAY_OF_WEEK");
            if (h.StartTime >= h.EndTime) return (null, "INVALID_TIME_RANGE");
        }

        var entities = list.Select(h => new BusinessHour
        {
            DayOfWeek = h.DayOfWeek,
            StartTime = h.StartTime,
            EndTime   = h.EndTime,
            IsActive  = h.IsActive
        });

        var saved = await hourRepository.ReplaceAsync(business.Id, entities);
        return (saved.Select(MapHour).ToList(), null);
    }

    public async Task<(IReadOnlyCollection<BusinessHourExceptionResponse>? Result, string? ErrorCode)> GetMyExceptionsAsync(Guid userId)
    {
        var business = await GetMyBusinessAsync(userId);
        if (business is null) return (null, "BUSINESS_NOT_FOUND");

        return ((await exceptionRepository.GetByBusinessIdAsync(business.Id)).Select(MapException).ToList(), null);
    }

    public async Task<(IReadOnlyCollection<BusinessHourExceptionResponse>? Result, string? ErrorCode)> ReplaceMyExceptionsAsync(
        Guid userId, IEnumerable<BusinessHourExceptionRequest> exceptions)
    {
        var business = await GetMyBusinessAsync(userId);
        if (business is null) return (null, "BUSINESS_NOT_FOUND");
        if (business.VerificationStatus != "approved") return (null, "BUSINESS_NOT_APPROVED");

        var list = exceptions.ToList();

        if (list.Select(e => e.Date).Distinct().Count() != list.Count)
            return (null, "DUPLICATE_DATE");

        foreach (var e in list)
        {
            if (!e.IsUnavailable && (e.StartTime is null || e.EndTime is null))
                return (null, "TIMES_REQUIRED_WHEN_AVAILABLE");
            if (e.StartTime is not null && e.EndTime is not null && e.StartTime >= e.EndTime)
                return (null, "INVALID_TIME_RANGE");
        }

        var entities = list.Select(e => new BusinessHourException
        {
            Date          = e.Date,
            IsUnavailable = e.IsUnavailable,
            StartTime     = e.StartTime,
            EndTime       = e.EndTime
        });

        var saved = await exceptionRepository.ReplaceAsync(business.Id, entities);
        return (saved.Select(MapException).ToList(), null);
    }

    private async Task<Business?> GetMyBusinessAsync(Guid userId) =>
        (await businessRepository.GetByOwnerIdAsync(userId)).FirstOrDefault();

    private static BusinessHourResponse MapHour(BusinessHour h) =>
        new(h.Id, h.BusinessId, h.DayOfWeek, h.StartTime, h.EndTime, h.IsActive);

    private static BusinessHourExceptionResponse MapException(BusinessHourException e) =>
        new(e.Id, e.BusinessId, e.Date, e.IsUnavailable, e.StartTime, e.EndTime);
}