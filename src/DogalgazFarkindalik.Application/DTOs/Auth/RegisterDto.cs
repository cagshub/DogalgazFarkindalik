using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.DTOs.Auth;

public record RegisterDto(
    string Email,
    string Password,
    string FullName,
    DateTime BirthDate,
    SubscriptionType SubscriptionType
);
