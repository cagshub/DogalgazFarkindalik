using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class ContentTargetingRule : BaseEntity
{
    public ModuleType Module { get; set; }
    public Guid? ReferenceId { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public SubscriptionType? SubscriptionType { get; set; }
    public double ScoreMultiplier { get; set; } = 1.0;
    public bool IsActive { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}
