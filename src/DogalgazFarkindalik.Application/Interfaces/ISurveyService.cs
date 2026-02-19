using DogalgazFarkindalik.Application.DTOs.Survey;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface ISurveyService
{
    Task<List<SurveyListDto>> GetActiveSurveysAsync(AgeGroup? ageGroup, SubscriptionType? subType, CancellationToken ct = default);
    Task<List<SurveyListDto>> GetAllSurveysAsync(CancellationToken ct = default);
    Task<SurveyDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SubmitResponsesAsync(Guid surveyId, Guid userId, SubmitSurveyResponseDto dto, CancellationToken ct = default);
    Task<SurveyDetailDto> CreateAsync(CreateSurveyDto dto, CancellationToken ct = default);
    Task<SurveyDetailDto> UpdateAsync(Guid id, CreateSurveyDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
