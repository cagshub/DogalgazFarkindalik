using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime BirthDate { get; set; }
    public AgeGroup AgeGroup { get; set; }
    public SubscriptionType SubscriptionType { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
