namespace BackEndPets.Application.Interfaces;

public record WalkGroupAccessResult(bool CanAccess, string? ErrorMessage);

public interface IWalkSessionAuthorizationService
{
    Task<WalkGroupAccessResult> CanJoinWalkGroupAsync(Guid userId, Guid sessionId);
}
