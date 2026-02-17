using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class SurveyQuestion : BaseEntity
{
    public Guid SurveyId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Weight { get; set; } = 1;
    public int Order { get; set; }
    public AgeGroup? AgeGroupFilter { get; set; }
    public SubscriptionType? SubscriptionFilter { get; set; }

    // Navigation
    public Survey Survey { get; set; } = null!;
    public ICollection<SurveyOption> Options { get; set; } = new List<SurveyOption>();
}
