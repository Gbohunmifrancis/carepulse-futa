using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FutaMedical.Infrastructure.BackgroundServices;

public class EmailQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public EmailQueueProcessor(IServiceProvider serviceProvider, ILogger<EmailQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Queue Processor service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing Email Queue Processor.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Email Queue Processor service stopped.");
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Get next pending emails scheduled for now or earlier
        var pendingEmails = await context.EmailQueues
            .Where(eq => eq.Status == "Pending" && eq.ScheduledFor <= DateTime.UtcNow)
            .OrderBy(eq => eq.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (!pendingEmails.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} pending emails to dispatch.", pendingEmails.Count);

        foreach (var email in pendingEmails)
        {
            email.Status = "Processing";
            email.Attempts++;
            await context.SaveChangesAsync(cancellationToken);

            try
            {
                _logger.LogInformation("Dispatching email ID {Id} to {Recipient} with subject: {Subject}", email.Id, email.To, email.Subject);
                await emailService.SendEmailAsync(email.To, email.Subject, email.Body);

                email.Status = "Completed";
                email.ProcessedAt = DateTime.UtcNow;
                email.ErrorMessage = null;
                _logger.LogInformation("Successfully dispatched email ID {Id}.", email.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch email ID {Id}.", email.Id);
                
                email.ErrorMessage = ex.Message;

                if (email.Attempts >= 3)
                {
                    email.Status = "Failed";
                    _logger.LogWarning("Email ID {Id} has exceeded max retry attempts (3). Marking as Failed.", email.Id);
                }
                else
                {
                    email.Status = "Pending";
                    // Exponential backoff: 30s, 2.5m, 12.5m
                    var backoffSeconds = (int)Math.Pow(5, email.Attempts) * 6;
                    email.ScheduledFor = DateTime.UtcNow.AddSeconds(backoffSeconds);
                    _logger.LogInformation("Email ID {Id} scheduled for retry in {Seconds} seconds.", email.Id, backoffSeconds);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
