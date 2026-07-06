using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IDeviceTokenRepository
{
    Task RegisterAsync(Guid userId, string token, string platform);
    Task RemoveAsync(string token);
    Task<IReadOnlyCollection<string>> GetTokensByUserIdAsync(Guid userId);
    Task RemoveTokensAsync(IReadOnlyCollection<string> tokens);
}
