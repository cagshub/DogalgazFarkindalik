using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.DTOs.Score;

public record ScoreDto(
    Guid Id, Guid UserId, Guid AttemptId, ModuleType Module,
    double RawScore, double SegmentMultiplier, double FinalScore,
    DateTime CreatedAt);
