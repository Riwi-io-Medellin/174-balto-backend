namespace BackEndPets.Application.DTOs.WalkingHistory;

public sealed record CreateWalkingHistoryRequest(
    Guid PetId,
    Guid WalkerId,
    DateTime? StartTime,
    decimal? Cost);

public sealed record WalkingHistoryResponse(
    Guid Id,
    Guid UserId,
    Guid PetId,
    Guid WalkerId,
    decimal? Cost,
    DateTime? StartTime,
    DateTime? EndTime,
    DateTime CreatedAt);