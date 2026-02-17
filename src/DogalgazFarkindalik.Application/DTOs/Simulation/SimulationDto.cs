using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.DTOs.Simulation;

public record SimulationListDto(
    Guid Id,
    string Title,
    string Description,
    AgeGroup MinAgeGroup,
    SubscriptionType? SubscriptionFilter
);

public record SimulationDetailDto(
    Guid Id,
    string Title,
    string Description,
    AgeGroup MinAgeGroup,
    SubscriptionType? SubscriptionFilter,
    List<SimulationQuestionDto> Questions
);

public record SimulationQuestionDto(
    Guid Id,
    string Text,
    string? ImageUrl,
    int Order,
    List<SimulationOptionDto> Options
);

public record SimulationOptionDto(Guid Id, string Text);

public record SubmitSimulationAnswerDto(Guid QuestionId, Guid SelectedOptionId);

public record SimulationResultDto(
    double Score,
    int TotalQuestions,
    int CorrectAnswers,
    List<QuestionResultDto> Details
);

public record QuestionResultDto(
    Guid QuestionId,
    string QuestionText,
    bool IsCorrect,
    string? Explanation,
    string CorrectAnswer
);

public record CreateSimulationDto(
    string Title,
    string Description,
    AgeGroup? MinAgeGroup,
    SubscriptionType? SubscriptionFilter,
    List<CreateSimulationQuestionDto> Questions
);

public record CreateSimulationQuestionDto(
    string Text,
    string? ImageUrl,
    int Order,
    List<CreateSimulationOptionDto> Options
);

public record CreateSimulationOptionDto(
    string Text,
    bool IsCorrect,
    string? Explanation
);
