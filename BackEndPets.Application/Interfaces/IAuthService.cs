using BackEndPets.Application.DTOs.Auth;

namespace BackEndPets.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    Task<(AuthResponse? Tokens, string? ErrorCode)> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(LogoutRequest request);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<(bool Success, string? ErrorCode)> ResetPasswordAsync(ResetPasswordRequest request);
    Task<(bool Success, string? ErrorCode)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<(AuthResponse? Tokens, string? ErrorCode)> GoogleLoginAsync(SocialLoginRequest request);
    Task<(AuthResponse? Tokens, string? ErrorCode)> AppleLoginAsync(SocialLoginRequest request);
}
