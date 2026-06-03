using Resend;
using FutaMedical.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FutaMedical.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public ResendEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        // Default to the provided API key, but read from configuration if available
        var apiKey = _configuration["Resend:ApiKey"] ?? "re_YDG2Q8iZ_JxuBFRWqhWqh12fQyqB8PJNk";
        var fromEmail = _configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";

        IResend resend = ResendClient.Create(apiKey);

        var message = new EmailMessage
        {
            From = fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(to);

        await resend.EmailSendAsync(message);
    }
}
