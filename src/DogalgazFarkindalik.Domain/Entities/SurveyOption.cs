using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class SurveyOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Value { get; set; }

    // Navigation
    public SurveyQuestion Question { get; set; } = null!;
}
