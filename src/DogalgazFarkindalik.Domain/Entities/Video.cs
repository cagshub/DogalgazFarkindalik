using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class Video : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int DurationSec { get; set; }
    public string Tags { get; set; } = string.Empty;
    public AgeGroup MinAgeGroup { get; set; }
    public SubscriptionType? SubscriptionFilter { get; set; }
    public bool IsPublished { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;

    // Navigation
    public ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
    public ICollection<VideoProgress> VideoProgresses { get; set; } = new List<VideoProgress>();
}
