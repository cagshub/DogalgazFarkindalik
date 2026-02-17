using DogalgazFarkindalik.Application.DTOs.Video;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface IVideoService
{
    Task<List<VideoDto>> GetVideosAsync(AgeGroup? ageGroup, SubscriptionType? subType, CancellationToken ct = default);
    Task<VideoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VideoDto> CreateAsync(CreateVideoDto dto, CancellationToken ct = default);
    Task<VideoDto> UpdateAsync(Guid id, CreateVideoDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
