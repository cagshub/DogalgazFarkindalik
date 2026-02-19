using DogalgazFarkindalik.Application.DTOs.Video;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Domain.Enums;
using DogalgazFarkindalik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class VideoService : IVideoService
{
    private readonly AppDbContext _context;

    public VideoService(AppDbContext context) => _context = context;

    public async Task<List<VideoDto>> GetVideosAsync(AgeGroup? ageGroup, SubscriptionType? subType, CancellationToken ct = default)
    {
        var query = _context.Videos.Where(v => v.IsPublished).AsQueryable();

        if (ageGroup.HasValue)
        {
            var allowedGroups = GetAllowedAgeGroups(ageGroup.Value);
            query = query.Where(v => allowedGroups.Contains(v.MinAgeGroup));
        }

        if (subType.HasValue)
            query = query.Where(v => v.SubscriptionFilter == null || v.SubscriptionFilter == subType.Value);

        return await query.Select(v => new VideoDto(
            v.Id, v.Title, v.Description, v.Url, v.DurationSec,
            v.Tags, v.MinAgeGroup, v.SubscriptionFilter, v.ThumbnailUrl
        )).ToListAsync(ct);
    }

    public async Task<VideoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var v = await _context.Videos.FindAsync([id], ct);
        if (v is null) return null;

        return new VideoDto(v.Id, v.Title, v.Description, v.Url, v.DurationSec,
            v.Tags, v.MinAgeGroup, v.SubscriptionFilter, v.ThumbnailUrl);
    }

    public async Task<VideoDto> CreateAsync(CreateVideoDto dto, CancellationToken ct = default)
    {
        var video = new Video
        {
            Title = dto.Title, Description = dto.Description, Url = dto.Url,
            DurationSec = dto.DurationSec, Tags = dto.Tags, MinAgeGroup = dto.MinAgeGroup,
            SubscriptionFilter = dto.SubscriptionFilter, ThumbnailUrl = dto.ThumbnailUrl,
            IsPublished = true
        };
        _context.Videos.Add(video);
        await _context.SaveChangesAsync(ct);

        return new VideoDto(video.Id, video.Title, video.Description, video.Url, video.DurationSec,
            video.Tags, video.MinAgeGroup, video.SubscriptionFilter, video.ThumbnailUrl);
    }

    public async Task<VideoDto> UpdateAsync(Guid id, CreateVideoDto dto, CancellationToken ct = default)
    {
        var video = await _context.Videos.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Video bulunamadi.");

        video.Title = dto.Title; video.Description = dto.Description; video.Url = dto.Url;
        video.DurationSec = dto.DurationSec; video.Tags = dto.Tags; video.MinAgeGroup = dto.MinAgeGroup;
        video.SubscriptionFilter = dto.SubscriptionFilter; video.ThumbnailUrl = dto.ThumbnailUrl;
        video.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return new VideoDto(video.Id, video.Title, video.Description, video.Url, video.DurationSec,
            video.Tags, video.MinAgeGroup, video.SubscriptionFilter, video.ThumbnailUrl);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var video = await _context.Videos.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Video bulunamadi.");
        _context.Videos.Remove(video);
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
}
