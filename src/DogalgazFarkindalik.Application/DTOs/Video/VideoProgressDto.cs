namespace DogalgazFarkindalik.Application.DTOs.Video;

public record VideoProgressDto(
    Guid VideoId, string VideoTitle, int WatchedSeconds,
    int TotalSeconds, double ProgressPercent, bool IsCompleted,
    DateTime LastWatchedAt);

public record UpdateVideoProgressDto(int WatchedSeconds, int TotalSeconds);
