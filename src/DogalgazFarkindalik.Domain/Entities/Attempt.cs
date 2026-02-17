using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class Attempt : BaseEntity
{
    public Guid UserId { get; set; }
    public ModuleType Module { get; set; }
    public Guid ReferenceId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public double? Score { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Score? ScoreDetail { get; set; }
}
