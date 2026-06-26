using BackEndPets.Application.DTOs.Auth;

namespace BackEndPets.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(LogoutRequest request);

    // ── Recuperación de contraseña ──────────────────────────────────────────
    /// <summary>
    /// Genera un token de reset y lo entrega al IEmailSender.
    /// Siempre retorna true para no revelar si el email existe.
    /// </summary>
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>Valida el token y establece la nueva contraseña.</summary>
    Task<(bool Success, string? ErrorCode)> ResetPasswordAsync(ResetPasswordRequest request);

    // ── Cambio desde perfil autenticado ─────────────────────────────────────
    /// <summary>Cambia la contraseña verificando la contraseña actual.</summary>
    Task<(bool Success, string? ErrorCode)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

    // ── Social login ─────────────────────────────────────────────────────────
    Task<(AuthResponse? Tokens, string? ErrorCode)> GoogleLoginAsync(SocialLoginRequest request);
    Task<(AuthResponse? Tokens, string? ErrorCode)> AppleLoginAsync(SocialLoginRequest request);
}
