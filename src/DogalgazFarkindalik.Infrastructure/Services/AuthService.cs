using DogalgazFarkindalik.Application.DTOs.Auth;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Domain.Enums;
using DogalgazFarkindalik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IJwtService jwtService, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email, ct))
            throw new InvalidOperationException("Bu e-posta adresi zaten kayitli.");

        var verificationToken = Guid.NewGuid().ToString("N");
        var expiryHours = int.Parse(_configuration["Email:VerificationTokenExpiryHours"] ?? "24");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Role = UserRole.User,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(expiryHours)
        };

        var birthDateUtc = DateTime.SpecifyKind(dto.BirthDate, DateTimeKind.Utc);
        var ageGroup = CalculateAgeGroup(birthDateUtc);
        var profile = new UserProfile
        {
            UserId = user.Id,
            BirthDate = birthDateUtc,
            AgeGroup = ageGroup,
            SubscriptionType = dto.SubscriptionType
        };

        _context.Users.Add(user);
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync(ct);

        // Send verification email (fire-and-forget for better UX, errors are logged)
        try
        {
            await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationToken, ct);
        }
        catch
        {
            // Email sending failure should not block registration
        }

        return new AuthResponseDto(
            AccessToken: string.Empty,
            RefreshToken: string.Empty,
            ExpiresAt: DateTime.UtcNow,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString(),
            IsEmailVerified: false
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        // Giris denemesi siniri kontrolu (son 15 dakikada 5 basarisiz deneme)
        var lockoutWindow = DateTime.UtcNow.AddMinutes(-15);
        var failedAttempts = await _context.LoginAttempts
            .CountAsync(la => la.Email == dto.Email && !la.IsSuccessful && la.AttemptedAt > lockoutWindow, ct);

        if (failedAttempts >= 5)
            throw new InvalidOperationException("Cok fazla basarisiz giris denemesi. Lutfen 15 dakika sonra tekrar deneyin.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            // Basarisiz denemeyi kaydet
            _context.LoginAttempts.Add(new LoginAttempt
            {
                Email = dto.Email,
                IsSuccessful = false,
                AttemptedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(ct);

            throw new UnauthorizedAccessException("E-posta veya sifre hatali.");
        }

        if (!user.IsEmailVerified)
            throw new InvalidOperationException("E-posta adresiniz henuz dogrulanmamis. Lutfen e-postanizi kontrol edin.");

        // Basarili denemeyi kaydet
        _context.LoginAttempts.Add(new LoginAttempt
        {
            Email = dto.Email,
            IsSuccessful = true,
            AttemptedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);

        return GenerateResponse(user);
    }

    public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        throw new NotImplementedException("Refresh token henuz implemente edilmedi.");
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.EmailVerificationToken == token, ct);

        if (user is null)
            return false;

        if (user.IsEmailVerified)
            return true;

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            return false;

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task ResendVerificationEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
            ?? throw new InvalidOperationException("Bu e-posta adresi ile kayitli kullanici bulunamadi.");

        if (user.IsEmailVerified)
            throw new InvalidOperationException("Bu e-posta adresi zaten dogrulanmis.");

        var verificationToken = Guid.NewGuid().ToString("N");
        var expiryHours = int.Parse(_configuration["Email:VerificationTokenExpiryHours"] ?? "24");

        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(expiryHours);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationToken, ct);
    }

    private AuthResponseDto GenerateResponse(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString(),
            IsEmailVerified: user.IsEmailVerified
        );
    }

    private static AgeGroup CalculateAgeGroup(DateTime birthDate)
    {
        var age = DateTime.UtcNow.Year - birthDate.Year;
        if (DateTime.UtcNow.DayOfYear < birthDate.DayOfYear) age--;

        return age switch
        {
            <= 12 => AgeGroup.Child,
            <= 65 => AgeGroup.Adult,
            _ => AgeGroup.Senior
        };
    }
}
