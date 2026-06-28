namespace BackEndPets.Application.DTOs.Walkers;

public sealed record WalkerApplyRequest(
    string WorkLocation,
    string Experience,
    string? Description);

public sealed record WalkerApplyResponse(
    Guid WalkerId,
    Guid UserId,
    string VerificationStatus,
    string DocumentUrl,
    string? DocumentName,
    string? DocumentNumber,
    string Message);
