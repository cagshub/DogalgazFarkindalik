using DogalgazFarkindalik.Application.DTOs.Video;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface IVideoProgressService
{
    Task<List<VideoProgressDto>> GetUserProgressAsync(Guid userId, CancellationToken ct = default);
    Task<VideoProgressDto> UpdateProgressAsync(Guid userId, Guid videoId, UpdateVideoProgressDto dto, CancellationToken ct = default);
}
