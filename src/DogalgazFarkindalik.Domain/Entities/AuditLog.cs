using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Meta { get; set; }

    // Navigation
    public User? User { get; set; }
}
