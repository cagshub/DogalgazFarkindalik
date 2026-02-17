using DogalgazFarkindalik.Application.DTOs.Auth;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default);
    Task ResendVerificationEmailAsync(string email, CancellationToken ct = default);
}
