using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.DTOs.Report;

public record SummaryReportDto(
    int TotalUsers,
    int TotalAttempts,
    double AverageScore,
    List<SegmentReportDto> ByAgeGroup,
    List<SegmentReportDto> BySubscription
);

public record SegmentReportDto(
    string Segment,
    int UserCount,
    int AttemptCount,
    double AverageScore
);
