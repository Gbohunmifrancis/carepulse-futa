using System.Threading.Tasks;

namespace FutaMedical.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
