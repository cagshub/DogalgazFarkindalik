using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.DTOs.Survey;

public record SurveyListDto(Guid Id, string Title, string Description);

public record SurveyDetailDto(
    Guid Id,
    string Title,
    string Description,
    List<SurveyQuestionDto> Questions
);

public record SurveyQuestionDto(
    Guid Id,
    string Text,
    QuestionType Type,
    int Order,
    List<SurveyOptionDto> Options
);

public record SurveyOptionDto(Guid Id, string Text);

public record SubmitSurveyResponseDto(List<SurveyAnswerDto> Answers);

public record SurveyAnswerDto(
    Guid QuestionId,
    Guid? SelectedOptionId,
    int? NumericValue
);

public record CreateSurveyDto(
    string Title,
    string Description,
    bool IsActive,
    List<CreateSurveyQuestionDto> Questions
);

public record CreateSurveyQuestionDto(
    string Text,
    QuestionType Type,
    int Weight,
    int Order,
    AgeGroup? AgeGroupFilter,
    SubscriptionType? SubscriptionFilter,
    List<CreateSurveyOptionDto> Options
);

public record CreateSurveyOptionDto(string Text, int Value);
