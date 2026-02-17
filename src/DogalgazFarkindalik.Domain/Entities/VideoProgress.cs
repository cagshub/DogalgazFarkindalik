using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class VideoProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid VideoId { get; set; }
    public int WatchedSeconds { get; set; }
    public int TotalSeconds { get; set; }
    public double ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastWatchedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Video Video { get; set; } = null!;
}
