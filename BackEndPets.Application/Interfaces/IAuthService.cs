using BackEndPets.Application.DTOs.Auth;

namespace BackEndPets.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);

    Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request);

    Task<bool> LogoutAsync(LogoutRequest request);
}
