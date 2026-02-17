using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.DTOs.ContentTargeting;

public record ContentTargetingRuleDto(
    Guid Id, ModuleType Module, Guid? ReferenceId,
    AgeGroup? AgeGroup, SubscriptionType? SubscriptionType,
    double ScoreMultiplier, bool IsActive, string Description);

public record CreateContentTargetingRuleDto(
    ModuleType Module, Guid? ReferenceId,
    AgeGroup? AgeGroup, SubscriptionType? SubscriptionType,
    double ScoreMultiplier, bool IsActive, string Description);
