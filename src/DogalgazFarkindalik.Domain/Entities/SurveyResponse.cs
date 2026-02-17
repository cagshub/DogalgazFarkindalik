using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class SurveyResponse : BaseEntity
{
    public Guid SurveyId { get; set; }
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public int? NumericValue { get; set; }

    // Navigation
    public Survey Survey { get; set; } = null!;
    public User User { get; set; } = null!;
    public SurveyQuestion Question { get; set; } = null!;
    public SurveyOption? SelectedOption { get; set; }
}
