using DogalgazFarkindalik.Domain.Common;

namespace DogalgazFarkindalik.Domain.Entities;

public class LoginAttempt : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public bool IsSuccessful { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
