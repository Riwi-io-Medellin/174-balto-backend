namespace BackEndPets.Application.Interfaces;

public record HomeServiceGroupAccessResult(bool CanAccess, string? ErrorMessage);

public interface IHomeServiceSessionAuthorizationService
{
    Task<HomeServiceGroupAccessResult> CanJoinSessionGroupAsync(Guid userId, Guid sessionId);
}
