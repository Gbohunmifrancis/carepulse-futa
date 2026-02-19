using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Infrastructure.Identity;
using FutaMedical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FutaMedical.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                builder => 
                {
                    builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    builder.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    builder.CommandTimeout(120); // 2 minutes
                })
                .EnableSensitiveDataLogging(bool.TryParse(configuration["EnableSensitiveDataLogging"], out var result) && result));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
