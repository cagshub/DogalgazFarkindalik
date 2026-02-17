using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class SimulationOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }

    // Navigation
    public SimulationQuestion Question { get; set; } = null!;
}
