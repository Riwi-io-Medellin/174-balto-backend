using BackEndPets.Application.DTOs.Admin;

namespace BackEndPets.Application.Interfaces;

public interface IAdminOperationsService
{
    Task<IReadOnlyCollection<AdminUserListItemResponse>> GetUsersAsync(
        string? search,
        string? status,
        string? role,
        DateTime? from,
        DateTime? to);

    Task<IReadOnlyCollection<AdminWalkerListItemResponse>> GetWalkersAsync(
        string? search,
        string? status,
        string? verificationStatus,
        DateTime? from,
        DateTime? to);

    Task<IReadOnlyCollection<AdminCommunityAlertResponse>> GetCommunityAlertsAsync(
        string? search,
        string? status,
        string? alertType,
        DateTime? from,
        DateTime? to);

    Task<AdminOperationResult<AdminUserListItemResponse>> UpdateUserAsync(
        Guid actorUserId,
        Guid userId,
        AdminEntityActionRequest request);

    Task<AdminOperationResult<AdminWalkerListItemResponse>> UpdateWalkerAsync(
        Guid actorUserId,
        Guid walkerId,
        AdminEntityActionRequest request);

    Task<AdminOperationResult<AdminCommunityAlertResponse>> ModerateAlertAsync(
        Guid actorUserId,
        Guid alertId,
        AdminAlertModerationRequest request);

    Task<IReadOnlyCollection<AdminAuditLogResponse>> GetAuditLogsAsync(
        string? entityType,
        Guid? entityId,
        int take);
}

public sealed record AdminOperationResult<T>(
    bool Success,
    string? ErrorCode,
    T? Data)
{
    public static AdminOperationResult<T> Ok(T data) => new(true, null, data);
    public static AdminOperationResult<T> Fail(string errorCode) => new(false, errorCode, default);
}
