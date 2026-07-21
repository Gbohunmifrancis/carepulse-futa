using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Infrastructure.Identity;
using FutaMedical.Infrastructure.Persistence;
using FutaMedical.Infrastructure.Services;
using FutaMedical.Infrastructure.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FutaMedical.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Heroku provides DATABASE_URL; parse it into a Npgsql connection string
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var port = uri.Port == -1 ? 5432 : uri.Port;
            connectionString = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                builder =>
                {
                    builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    builder.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    builder.CommandTimeout(120);
                })
                .EnableSensitiveDataLogging(
                    bool.TryParse(configuration["EnableSensitiveDataLogging"], out var result) && result));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailService, SmtpEmailService>();   // ← Google SMTP
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddHostedService<EmailQueueProcessor>();

        return services;
    }
}
