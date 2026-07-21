using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FutaMedical.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:User"]
            ?? throw new InvalidOperationException("SMTP username (Smtp:User) is not configured.");
        var smtpPassword = _configuration["Smtp:Password"]
            ?? throw new InvalidOperationException("SMTP password (Smtp:Password) is not configured.");
        var fromEmail = _configuration["Smtp:FromEmail"] ?? smtpUser;
        var fromName = _configuration["Smtp:FromName"] ?? "FUTA Medical";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPassword),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(to));

        _logger.LogInformation("Sending email via Google SMTP to {Recipient} with subject: {Subject}", to, subject);
        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent successfully to {Recipient}.", to);
    }
}
