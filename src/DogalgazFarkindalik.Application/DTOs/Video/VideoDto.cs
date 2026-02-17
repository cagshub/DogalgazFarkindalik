using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.DTOs.Video;

public record VideoDto(
    Guid Id,
    string Title,
    string Description,
    string Url,
    int DurationSec,
    string Tags,
    AgeGroup MinAgeGroup,
    SubscriptionType? SubscriptionFilter,
    string ThumbnailUrl
);

public record CreateVideoDto(
    string Title,
    string Description,
    string Url,
    int DurationSec,
    string Tags,
    AgeGroup MinAgeGroup,
    SubscriptionType? SubscriptionFilter,
    string ThumbnailUrl
);
