using DogalgazFarkindalik.Domain.Common;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;

    // Email verification
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    // Refresh token
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Navigation
    public UserProfile? Profile { get; set; }
    public ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<VideoProgress> VideoProgresses { get; set; } = new List<VideoProgress>();
}
