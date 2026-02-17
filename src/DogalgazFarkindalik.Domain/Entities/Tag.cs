using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
}
