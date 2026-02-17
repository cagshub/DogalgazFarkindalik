using DogalgazFarkindalik.Application.DTOs.ContentTargeting;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class ContentTargetingRuleService : IContentTargetingRuleService
{
    private readonly AppDbContext _context;

    public ContentTargetingRuleService(AppDbContext context) => _context = context;

    public async Task<List<ContentTargetingRuleDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ContentTargetingRules
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ContentTargetingRuleDto(
                r.Id, r.Module, r.ReferenceId,
                r.AgeGroup, r.SubscriptionType,
                r.ScoreMultiplier, r.IsActive, r.Description))
            .ToListAsync(ct);
    }

    public async Task<ContentTargetingRuleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _context.ContentTargetingRules.FindAsync([id], ct);
        if (r is null) return null;

        return new ContentTargetingRuleDto(
            r.Id, r.Module, r.ReferenceId,
            r.AgeGroup, r.SubscriptionType,
            r.ScoreMultiplier, r.IsActive, r.Description);
    }

    public async Task<ContentTargetingRuleDto> CreateAsync(CreateContentTargetingRuleDto dto, CancellationToken ct = default)
    {
        var rule = new ContentTargetingRule
        {
            Module = dto.Module,
            ReferenceId = dto.ReferenceId,
            AgeGroup = dto.AgeGroup,
            SubscriptionType = dto.SubscriptionType,
            ScoreMultiplier = dto.ScoreMultiplier,
            IsActive = dto.IsActive,
            Description = dto.Description
        };

        _context.ContentTargetingRules.Add(rule);
        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(rule.Id, ct))!;
    }

    public async Task<ContentTargetingRuleDto> UpdateAsync(Guid id, CreateContentTargetingRuleDto dto, CancellationToken ct = default)
    {
        var rule = await _context.ContentTargetingRules.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Hedefleme kurali bulunamadi.");

        rule.Module = dto.Module;
        rule.ReferenceId = dto.ReferenceId;
        rule.AgeGroup = dto.AgeGroup;
        rule.SubscriptionType = dto.SubscriptionType;
        rule.ScoreMultiplier = dto.ScoreMultiplier;
        rule.IsActive = dto.IsActive;
        rule.Description = dto.Description;
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _context.ContentTargetingRules.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Hedefleme kurali bulunamadi.");

        _context.ContentTargetingRules.Remove(rule);
        await _context.SaveChangesAsync(ct);
    }
}
