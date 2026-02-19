using DogalgazFarkindalik.Application.DTOs.Simulation;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Domain.Enums;
using DogalgazFarkindalik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class SimulationService : ISimulationService
{
    private readonly AppDbContext _context;

    public SimulationService(AppDbContext context) => _context = context;

    public async Task<List<SimulationListDto>> GetSimulationsAsync(AgeGroup? ageGroup, SubscriptionType? subType, CancellationToken ct = default)
    {
        var query = _context.Simulations.Where(s => s.IsPublished).AsQueryable();

        if (ageGroup.HasValue)
        {
            var allowedGroups = GetAllowedAgeGroups(ageGroup.Value);
            query = query.Where(s => allowedGroups.Contains(s.MinAgeGroup));
        }
        if (subType.HasValue)
            query = query.Where(s => s.SubscriptionFilter == null || s.SubscriptionFilter == subType.Value);

        return await query.Select(s => new SimulationListDto(
            s.Id, s.Title, s.Description, s.MinAgeGroup, s.SubscriptionFilter
        )).ToListAsync(ct);
    }

    public async Task<SimulationDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sim = await _context.Simulations
            .Include(s => s.Questions.OrderBy(q => q.Order))
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sim is null) return null;

        return new SimulationDetailDto(
            sim.Id, sim.Title, sim.Description, sim.MinAgeGroup, sim.SubscriptionFilter,
            sim.Questions.Select(q => new SimulationQuestionDto(
                q.Id, q.Text, q.ImageUrl, q.Order,
                q.Options.Select(o => new SimulationOptionDto(o.Id, o.Text, o.IsCorrect, o.Explanation)).ToList()
            )).ToList()
        );
    }

    public async Task<SimulationResultDto> SubmitAnswersAsync(Guid simulationId, Guid userId, List<SubmitSimulationAnswerDto> answers, CancellationToken ct = default)
    {
        var sim = await _context.Simulations
            .Include(s => s.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == simulationId, ct)
            ?? throw new KeyNotFoundException("Simulasyon bulunamadi.");

        var details = new List<QuestionResultDto>();
        int correct = 0;

        foreach (var answer in answers)
        {
            var question = sim.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question is null) continue;

            var selectedOption = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId);
            var correctOption = question.Options.First(o => o.IsCorrect);
            var isCorrect = selectedOption?.IsCorrect ?? false;

            if (isCorrect) correct++;

            details.Add(new QuestionResultDto(
                question.Id, question.Text, isCorrect,
                selectedOption?.Explanation ?? correctOption.Explanation,
                correctOption.Text
            ));
        }

        var rawScore = sim.Questions.Count > 0 ? (double)correct / sim.Questions.Count * 100 : 0;

        // Segment carpanini uygula
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        var segmentMultiplier = GetSegmentMultiplier(userProfile?.AgeGroup, userProfile?.SubscriptionType);
        var finalScore = Math.Min(rawScore * segmentMultiplier, 100);

        // Attempt kaydi
        var attempt = new Attempt
        {
            UserId = userId,
            Module = ModuleType.Simulation,
            ReferenceId = simulationId,
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
            Module = ModuleType.Simulation,
            RawScore = rawScore,
            SegmentMultiplier = segmentMultiplier,
            FinalScore = finalScore
        });

        await _context.SaveChangesAsync(ct);

        return new SimulationResultDto(finalScore, sim.Questions.Count, correct, details);
    }

    public async Task<SimulationDetailDto> CreateAsync(CreateSimulationDto dto, CancellationToken ct = default)
    {
        var simulation = new Simulation
        {
            Title = dto.Title,
            Description = dto.Description,
            MinAgeGroup = dto.MinAgeGroup ?? AgeGroup.Child,
            SubscriptionFilter = dto.SubscriptionFilter,
            IsPublished = true
        };

        foreach (var qDto in dto.Questions)
        {
            var question = new SimulationQuestion
            {
                Text = qDto.Text,
                ImageUrl = qDto.ImageUrl,
                Order = qDto.Order
            };

            foreach (var oDto in qDto.Options)
            {
                question.Options.Add(new SimulationOption
                {
                    Text = oDto.Text,
                    IsCorrect = oDto.IsCorrect,
                    Explanation = oDto.Explanation
                });
            }

            simulation.Questions.Add(question);
        }

        _context.Simulations.Add(simulation);
        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(simulation.Id, ct))!;
    }

    public async Task<SimulationDetailDto> UpdateAsync(Guid id, CreateSimulationDto dto, CancellationToken ct = default)
    {
        var simulation = await _context.Simulations
            .Include(s => s.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new KeyNotFoundException("Simulasyon bulunamadi.");

        simulation.Title = dto.Title;
        simulation.Description = dto.Description;
        simulation.MinAgeGroup = dto.MinAgeGroup ?? AgeGroup.Child;
        simulation.SubscriptionFilter = dto.SubscriptionFilter;

        // Remove existing questions (cascade deletes options)
        _context.SimulationQuestions.RemoveRange(simulation.Questions);

        // Add new questions
        foreach (var qDto in dto.Questions)
        {
            var question = new SimulationQuestion
            {
                SimulationId = id,
                Text = qDto.Text,
                ImageUrl = qDto.ImageUrl,
                Order = qDto.Order
            };

            foreach (var oDto in qDto.Options)
            {
                question.Options.Add(new SimulationOption
                {
                    Text = oDto.Text,
                    IsCorrect = oDto.IsCorrect,
                    Explanation = oDto.Explanation
                });
            }

            _context.SimulationQuestions.Add(question);
        }

        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var simulation = await _context.Simulations.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Simulasyon bulunamadi.");

        _context.Simulations.Remove(simulation);
        await _context.SaveChangesAsync(ct);
    }

    private static List<AgeGroup> GetAllowedAgeGroups(AgeGroup userGroup)
    {
        return userGroup switch
        {
            AgeGroup.Child => [AgeGroup.Child],
            AgeGroup.Adult => [AgeGroup.Child, AgeGroup.Adult],
            AgeGroup.Senior => [AgeGroup.Child, AgeGroup.Adult, AgeGroup.Senior],
            _ => [AgeGroup.Child]
        };
    }

    private static double GetSegmentMultiplier(AgeGroup? ageGroup, SubscriptionType? subType)
    {
        double multiplier = 1.0;

        // Yas grubu carpani
        multiplier *= ageGroup switch
        {
            AgeGroup.Child => 1.0,
            AgeGroup.Adult => 1.0,
            AgeGroup.Senior => 1.2, // 65+ guvenlik sorularinda x1.2
            _ => 1.0
        };

        // Abonelik tipi carpani
        multiplier *= subType switch
        {
            SubscriptionType.Endustriyel => 1.1, // Endustriyel icerikler icin x1.1
            _ => 1.0
        };

        return multiplier;
    }
}
