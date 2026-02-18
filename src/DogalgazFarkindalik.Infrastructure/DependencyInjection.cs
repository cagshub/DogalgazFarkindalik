using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Interfaces;
using DogalgazFarkindalik.Infrastructure.Data;
using DogalgazFarkindalik.Infrastructure.Repositories;
using DogalgazFarkindalik.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DogalgazFarkindalik.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // HttpClient (Resend API vb. icin)
        services.AddHttpClient();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<ISimulationService, SimulationService>();
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IContentTargetingRuleService, ContentTargetingRuleService>();
        services.AddScoped<IVideoProgressService, VideoProgressService>();

        return services;
    }
}
