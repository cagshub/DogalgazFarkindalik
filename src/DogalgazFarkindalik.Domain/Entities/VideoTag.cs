namespace DogalgazFarkindalik.Domain.Entities;

public class VideoTag
{
    public Guid VideoId { get; set; }
    public Guid TagId { get; set; }

    // Navigation
    public Video Video { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
