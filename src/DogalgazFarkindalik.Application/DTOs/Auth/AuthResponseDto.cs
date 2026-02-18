namespace DogalgazFarkindalik.Application.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Email,
    string FullName,
    string Role,
    bool IsEmailVerified,
    string? AgeGroup = null,
    string? SubscriptionType = null
);
