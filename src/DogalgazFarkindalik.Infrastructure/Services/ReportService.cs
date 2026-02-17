using DogalgazFarkindalik.Application.DTOs.Report;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context) => _context = context;

    public async Task<SummaryReportDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var totalUsers = await _context.Users.CountAsync(ct);
        var totalAttempts = await _context.Attempts.CountAsync(ct);
        var averageScore = totalAttempts > 0
            ? await _context.Attempts.Where(a => a.Score.HasValue).AverageAsync(a => a.Score!.Value, ct)
            : 0;

        var byAgeGroup = await _context.UserProfiles
            .GroupBy(p => p.AgeGroup)
            .Select(g => new SegmentReportDto(
                g.Key.ToString(),
                g.Count(),
                _context.Attempts.Count(a => g.Select(p => p.UserId).Contains(a.UserId)),
                _context.Attempts
                    .Where(a => g.Select(p => p.UserId).Contains(a.UserId) && a.Score.HasValue)
                    .Average(a => (double?)a.Score!.Value) ?? 0
            ))
            .ToListAsync(ct);

        var bySubscription = await _context.UserProfiles
            .GroupBy(p => p.SubscriptionType)
            .Select(g => new SegmentReportDto(
                g.Key.ToString(),
                g.Count(),
                _context.Attempts.Count(a => g.Select(p => p.UserId).Contains(a.UserId)),
                _context.Attempts
                    .Where(a => g.Select(p => p.UserId).Contains(a.UserId) && a.Score.HasValue)
                    .Average(a => (double?)a.Score!.Value) ?? 0
            ))
            .ToListAsync(ct);

        return new SummaryReportDto(totalUsers, totalAttempts, averageScore, byAgeGroup, bySubscription);
    }
}
