using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class SimulationQuestion : BaseEntity
{
    public Guid SimulationId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Order { get; set; }

    // Navigation
    public Simulation Simulation { get; set; } = null!;
    public ICollection<SimulationOption> Options { get; set; } = new List<SimulationOption>();
}
