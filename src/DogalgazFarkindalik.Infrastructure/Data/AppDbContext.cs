using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Simulation> Simulations => Set<Simulation>();
    public DbSet<SimulationQuestion> SimulationQuestions => Set<SimulationQuestion>();
    public DbSet<SimulationOption> SimulationOptions => Set<SimulationOption>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyQuestion> SurveyQuestions => Set<SurveyQuestion>();
    public DbSet<SurveyOption> SurveyOptions => Set<SurveyOption>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<VideoTag> VideoTags => Set<VideoTag>();
    public DbSet<ContentTargetingRule> ContentTargetingRules => Set<ContentTargetingRule>();
    public DbSet<VideoProgress> VideoProgresses => Set<VideoProgress>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        // UserProfile
        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasOne(p => p.User).WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.AgeGroup).HasConversion<string>();
            e.Property(p => p.SubscriptionType).HasConversion<string>();
        });

        // Video
        modelBuilder.Entity<Video>(e =>
        {
            e.Property(v => v.MinAgeGroup).HasConversion<string>();
            e.Property(v => v.SubscriptionFilter).HasConversion<string>();
        });

        // Simulation
        modelBuilder.Entity<Simulation>(e =>
        {
            e.Property(s => s.MinAgeGroup).HasConversion<string>();
            e.Property(s => s.SubscriptionFilter).HasConversion<string>();
            e.HasMany(s => s.Questions).WithOne(q => q.Simulation)
                .HasForeignKey(q => q.SimulationId).OnDelete(DeleteBehavior.Cascade);
        });

        // SimulationQuestion
        modelBuilder.Entity<SimulationQuestion>(e =>
        {
            e.HasMany(q => q.Options).WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        // Survey
        modelBuilder.Entity<Survey>(e =>
        {
            e.HasMany(s => s.Questions).WithOne(q => q.Survey)
                .HasForeignKey(q => q.SurveyId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(s => s.Responses).WithOne(r => r.Survey)
                .HasForeignKey(r => r.SurveyId).OnDelete(DeleteBehavior.Cascade);
        });

        // SurveyQuestion
        modelBuilder.Entity<SurveyQuestion>(e =>
        {
            e.Property(q => q.Type).HasConversion<string>();
            e.Property(q => q.AgeGroupFilter).HasConversion<string>();
            e.Property(q => q.SubscriptionFilter).HasConversion<string>();
            e.HasMany(q => q.Options).WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        // SurveyResponse
        modelBuilder.Entity<SurveyResponse>(e =>
        {
            e.HasOne(r => r.User).WithMany(u => u.SurveyResponses)
                .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Question).WithMany()
                .HasForeignKey(r => r.QuestionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.SelectedOption).WithMany()
                .HasForeignKey(r => r.SelectedOptionId).OnDelete(DeleteBehavior.SetNull);
        });

        // Attempt
        modelBuilder.Entity<Attempt>(e =>
        {
            e.Property(a => a.Module).HasConversion<string>();
            e.HasOne(a => a.User).WithMany(u => u.Attempts)
                .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasOne(a => a.User).WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        // Score
        modelBuilder.Entity<Score>(e =>
        {
            e.Property(s => s.Module).HasConversion<string>();
            e.HasOne(s => s.User).WithMany(u => u.Scores)
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Attempt).WithOne(a => a.ScoreDetail)
                .HasForeignKey<Score>(s => s.AttemptId).OnDelete(DeleteBehavior.Cascade);
        });

        // Tag
        modelBuilder.Entity<Tag>(e =>
        {
            e.HasIndex(t => t.Name).IsUnique();
        });

        // VideoTag (many-to-many join)
        modelBuilder.Entity<VideoTag>(e =>
        {
            e.HasKey(vt => new { vt.VideoId, vt.TagId });
            e.HasOne(vt => vt.Video).WithMany(v => v.VideoTags)
                .HasForeignKey(vt => vt.VideoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(vt => vt.Tag).WithMany(t => t.VideoTags)
                .HasForeignKey(vt => vt.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        // ContentTargetingRule
        modelBuilder.Entity<ContentTargetingRule>(e =>
        {
            e.Property(r => r.Module).HasConversion<string>();
            e.Property(r => r.AgeGroup).HasConversion<string>();
            e.Property(r => r.SubscriptionType).HasConversion<string>();
        });

        // VideoProgress
        modelBuilder.Entity<VideoProgress>(e =>
        {
            e.HasOne(vp => vp.User).WithMany(u => u.VideoProgresses)
                .HasForeignKey(vp => vp.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(vp => vp.Video).WithMany(v => v.VideoProgresses)
                .HasForeignKey(vp => vp.VideoId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(vp => new { vp.UserId, vp.VideoId }).IsUnique();
        });

        // LoginAttempt
        modelBuilder.Entity<LoginAttempt>(e =>
        {
            e.HasIndex(la => la.Email);
        });
    }
}
