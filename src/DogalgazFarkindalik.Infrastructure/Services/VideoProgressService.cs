using DogalgazFarkindalik.Application.DTOs.Video;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class VideoProgressService : IVideoProgressService
{
    private readonly AppDbContext _context;

    public VideoProgressService(AppDbContext context) => _context = context;

    public async Task<List<VideoProgressDto>> GetUserProgressAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.VideoProgresses
            .Include(vp => vp.Video)
            .Where(vp => vp.UserId == userId)
            .OrderByDescending(vp => vp.LastWatchedAt)
            .Select(vp => new VideoProgressDto(
                vp.VideoId, vp.Video.Title, vp.WatchedSeconds,
                vp.TotalSeconds, vp.ProgressPercent, vp.IsCompleted,
                vp.LastWatchedAt))
            .ToListAsync(ct);
    }

    public async Task<VideoProgressDto> UpdateProgressAsync(Guid userId, Guid videoId, UpdateVideoProgressDto dto, CancellationToken ct = default)
    {
        var video = await _context.Videos.FindAsync([videoId], ct)
            ?? throw new KeyNotFoundException("Video bulunamadi.");

        var progress = await _context.VideoProgresses
            .FirstOrDefaultAsync(vp => vp.UserId == userId && vp.VideoId == videoId, ct);

        var totalSeconds = dto.TotalSeconds > 0 ? dto.TotalSeconds : video.DurationSec;
        var progressPercent = totalSeconds > 0 ? (double)dto.WatchedSeconds / totalSeconds * 100 : 0;
        var isCompleted = progressPercent >= 90;

        if (progress is null)
        {
            progress = new VideoProgress
            {
                UserId = userId,
                VideoId = videoId,
                WatchedSeconds = dto.WatchedSeconds,
                TotalSeconds = totalSeconds,
                ProgressPercent = progressPercent,
                IsCompleted = isCompleted,
                LastWatchedAt = DateTime.UtcNow
            };
            _context.VideoProgresses.Add(progress);
        }
        else
        {
            progress.WatchedSeconds = dto.WatchedSeconds;
            progress.TotalSeconds = totalSeconds;
            progress.ProgressPercent = progressPercent;
            progress.IsCompleted = isCompleted;
            progress.LastWatchedAt = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        return new VideoProgressDto(
            videoId, video.Title, progress.WatchedSeconds,
            progress.TotalSeconds, progress.ProgressPercent,
            progress.IsCompleted, progress.LastWatchedAt);
    }
}
