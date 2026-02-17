using DogalgazFarkindalik.Application.DTOs.Survey;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Domain.Enums;
using DogalgazFarkindalik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class SurveyService : ISurveyService
{
    private readonly AppDbContext _context;

    public SurveyService(AppDbContext context) => _context = context;

    public async Task<List<SurveyListDto>> GetActiveSurveysAsync(AgeGroup? ageGroup, SubscriptionType? subType, CancellationToken ct = default)
    {
        var query = _context.Surveys.Where(s => s.IsActive).AsQueryable();

        if (ageGroup.HasValue || subType.HasValue)
        {
            var surveyIds = await _context.SurveyQuestions
                .Where(q =>
                    (!ageGroup.HasValue || q.AgeGroupFilter == null || q.AgeGroupFilter == ageGroup.Value) &&
                    (!subType.HasValue || q.SubscriptionFilter == null || q.SubscriptionFilter == subType.Value))
                .Select(q => q.SurveyId)
                .Distinct()
                .ToListAsync(ct);

            query = query.Where(s => surveyIds.Contains(s.Id));
        }

        return await query.Select(s => new SurveyListDto(s.Id, s.Title, s.Description))
            .ToListAsync(ct);
    }

    public async Task<SurveyDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var survey = await _context.Surveys
            .Include(s => s.Questions.OrderBy(q => q.Order))
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (survey is null) return null;

        return new SurveyDetailDto(
            survey.Id, survey.Title, survey.Description,
            survey.Questions.Select(q => new SurveyQuestionDto(
                q.Id, q.Text, q.Type, q.Order,
                q.Options.Select(o => new SurveyOptionDto(o.Id, o.Text)).ToList()
            )).ToList()
        );
    }

    public async Task SubmitResponsesAsync(Guid surveyId, Guid userId, SubmitSurveyResponseDto dto, CancellationToken ct = default)
    {
        var survey = await _context.Surveys
            .Include(s => s.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == surveyId, ct)
            ?? throw new KeyNotFoundException("Anket bulunamadı.");

        double totalWeight = 0;
        double earnedScore = 0;

        foreach (var answer in dto.Answers)
        {
            var question = survey.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question is null) continue;

            var response = new SurveyResponse
            {
                SurveyId = surveyId,
                UserId = userId,
                QuestionId = answer.QuestionId,
                SelectedOptionId = answer.SelectedOptionId,
                NumericValue = answer.NumericValue
            };
            _context.SurveyResponses.Add(response);

            totalWeight += question.Weight;

            if (answer.SelectedOptionId.HasValue)
            {
                var option = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId.Value);
                if (option is not null)
                    earnedScore += (option.Value / 100.0) * question.Weight;
            }
            else if (answer.NumericValue.HasValue && question.Type == QuestionType.Scale)
            {
                earnedScore += (answer.NumericValue.Value / 10.0) * question.Weight;
            }
        }

        var rawScore = totalWeight > 0 ? (earnedScore / totalWeight) * 100 : 0;

        // Segment carpanini uygula
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        var segmentMultiplier = GetSegmentMultiplier(userProfile?.AgeGroup, userProfile?.SubscriptionType);
        var finalScore = Math.Min(rawScore * segmentMultiplier, 100);

        var attempt = new Attempt
        {
            UserId = userId,
            Module = ModuleType.Survey,
            ReferenceId = surveyId,
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow,
            Score = finalScore
        };
        _context.Attempts.Add(attempt);

        // Score kaydi
        _context.Scores.Add(new Score
        {
            UserId = userId,
            AttemptId = attempt.Id,
            Module = ModuleType.Survey,
            RawScore = rawScore,
            SegmentMultiplier = segmentMultiplier,
            FinalScore = finalScore
        });

        await _context.SaveChangesAsync(ct);
    }

    public async Task<SurveyDetailDto> CreateAsync(CreateSurveyDto dto, CancellationToken ct = default)
    {
        var survey = new Survey
        {
            Title = dto.Title,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        foreach (var qDto in dto.Questions)
        {
            var question = new SurveyQuestion
            {
                Text = qDto.Text,
                Type = qDto.Type,
                Weight = qDto.Weight,
                Order = qDto.Order,
                AgeGroupFilter = qDto.AgeGroupFilter,
                SubscriptionFilter = qDto.SubscriptionFilter
            };

            foreach (var oDto in qDto.Options)
            {
                question.Options.Add(new SurveyOption
                {
                    Text = oDto.Text,
                    Value = oDto.Value
                });
            }

            survey.Questions.Add(question);
        }

        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(survey.Id, ct))!;
    }

    public async Task<SurveyDetailDto> UpdateAsync(Guid id, CreateSurveyDto dto, CancellationToken ct = default)
    {
        var survey = await _context.Surveys
            .Include(s => s.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new KeyNotFoundException("Anket bulunamadi.");

        survey.Title = dto.Title;
        survey.Description = dto.Description;
        survey.IsActive = dto.IsActive;

        // Remove existing questions (cascade deletes options)
        _context.SurveyQuestions.RemoveRange(survey.Questions);

        // Add new questions
        foreach (var qDto in dto.Questions)
        {
            var question = new SurveyQuestion
            {
                SurveyId = id,
                Text = qDto.Text,
                Type = qDto.Type,
                Weight = qDto.Weight,
                Order = qDto.Order,
                AgeGroupFilter = qDto.AgeGroupFilter,
                SubscriptionFilter = qDto.SubscriptionFilter
            };

            foreach (var oDto in qDto.Options)
            {
                question.Options.Add(new SurveyOption
                {
                    Text = oDto.Text,
                    Value = oDto.Value
                });
            }

            _context.SurveyQuestions.Add(question);
        }

        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var survey = await _context.Surveys.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Anket bulunamadi.");

        _context.Surveys.Remove(survey);
        await _context.SaveChangesAsync(ct);
    }

    private static double GetSegmentMultiplier(AgeGroup? ageGroup, SubscriptionType? subType)
    {
        double multiplier = 1.0;

        multiplier *= ageGroup switch
        {
            AgeGroup.Child => 1.0,
            AgeGroup.Adult => 1.0,
            AgeGroup.Senior => 1.2,
            _ => 1.0
        };

        multiplier *= subType switch
        {
            SubscriptionType.Endustriyel => 1.1,
            _ => 1.0
        };

        return multiplier;
    }
}
