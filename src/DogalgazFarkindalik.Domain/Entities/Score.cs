using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class Score : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AttemptId { get; set; }
    public ModuleType Module { get; set; }
    public double RawScore { get; set; }
    public double SegmentMultiplier { get; set; } = 1.0;
    public double FinalScore { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Attempt Attempt { get; set; } = null!;
}
