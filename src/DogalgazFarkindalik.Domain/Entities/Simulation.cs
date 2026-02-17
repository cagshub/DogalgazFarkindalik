using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class Simulation : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AgeGroup MinAgeGroup { get; set; }
    public SubscriptionType? SubscriptionFilter { get; set; }
    public bool IsPublished { get; set; }

    // Navigation
    public ICollection<SimulationQuestion> Questions { get; set; } = new List<SimulationQuestion>();
}
