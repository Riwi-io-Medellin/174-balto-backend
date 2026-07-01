namespace BackEndPets.Application.DTOs.Admin;

public sealed record AdminUserListItemResponse(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string? Location,
    string Role,
    string Status,
    string RiskLevel,
    DateTime CreatedAt);

public sealed record AdminWalkerListItemResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? WorkLocation,
    string VerificationStatus,
    string Status,
    string RiskLevel,
    bool IsAcceptingBookings,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AdminCommunityAlertResponse(
    Guid Id,
    Guid ReporterUserId,
    string ReporterName,
    string AlertType,
    string PetName,
    string? Species,
    string Description,
    string? LastSeenLocation,
    string? EvidenceUrl,
    string Status,
    string RiskLevel,
    string? ModerationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AdminAuditLogResponse(
    Guid Id,
    Guid ActorUserId,
    string Action,
    string EntityType,
    Guid EntityId,
    string Reason,
    DateTime CreatedAt);

public sealed record AdminEntityActionRequest(
    string Action,
    string Reason,
    bool ConfirmImpact);

public sealed record AdminAlertModerationRequest(
    string Status,
    string Reason,
    DateTime ExpectedUpdatedAt,
    bool ConfirmImpact);
