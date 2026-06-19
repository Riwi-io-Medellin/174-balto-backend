using BackEndPets.Application.DTOs.Auth;

namespace BackEndPets.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);

    Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request);

    Task<bool> LogoutAsync(LogoutRequest request);
}
