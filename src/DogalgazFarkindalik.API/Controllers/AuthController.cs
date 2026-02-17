using DogalgazFarkindalik.Application.DTOs.Auth;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Kimlik dogrulama islemleri (kayit, giris, e-posta dogrulama)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Yeni kullanici kaydi olusturur ve dogrulama maili gonderir
    /// </summary>
    /// <param name="dto">Kayit bilgileri (email, sifre, ad soyad, dogum tarihi, abonelik tipi)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Kayit sonucu ve kullanici bilgileri</returns>
    /// <response code="200">Kayit basarili, dogrulama maili gonderildi</response>
    /// <response code="400">Gecersiz veri</response>
    /// <response code="409">Bu e-posta adresi zaten kayitli</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto, ct);
            return Ok(new
            {
                message = "Kayit basarili! Lutfen e-posta adresinize gonderilen dogrulama linkine tiklayiniz.",
                email = result.Email,
                fullName = result.FullName,
                isEmailVerified = result.IsEmailVerified
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanici girisi yapar ve JWT token doner
    /// </summary>
    /// <param name="dto">Giris bilgileri (email, sifre)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>JWT access token, refresh token ve kullanici bilgileri</returns>
    /// <response code="200">Giris basarili, JWT token doner</response>
    /// <response code="400">E-posta dogrulanmamis</response>
    /// <response code="401">E-posta veya sifre hatali</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _authService.LoginAsync(dto, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, emailNotVerified = true });
        }
    }

    /// <summary>
    /// E-posta dogrulama tokeni ile e-posta adresini dogrular
    /// </summary>
    /// <param name="token">Dogrulama tokeni (kayit sirasinda e-posta ile gonderilir)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dogrulama sonucu</returns>
    /// <response code="200">E-posta basariyla dogrulandi</response>
    /// <response code="400">Gecersiz veya suresi dolmus token</response>
    [HttpGet("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Dogrulama tokeni gereklidir." });

        var result = await _authService.VerifyEmailAsync(token, ct);

        if (result)
            return Ok(new { message = "E-posta adresiniz basariyla dogrulandi! Simdi giris yapabilirsiniz." });

        return BadRequest(new { message = "Gecersiz veya suresi dolmus dogrulama tokeni." });
    }

    /// <summary>
    /// Dogrulama mailini tekrar gonderir
    /// </summary>
    /// <param name="dto">E-posta adresi</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Gonderim sonucu</returns>
    /// <response code="200">Dogrulama maili tekrar gonderildi</response>
    /// <response code="400">Kullanici bulunamadi veya zaten dogrulanmis</response>
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto, CancellationToken ct)
    {
        try
        {
            await _authService.ResendVerificationEmailAsync(dto.Email, ct);
            return Ok(new { message = "Dogrulama maili tekrar gonderildi. Lutfen e-posta kutunuzu kontrol edin." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record ResendVerificationDto(string Email);
