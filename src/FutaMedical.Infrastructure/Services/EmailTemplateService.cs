using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IApplicationDbContext _context;

    public EmailTemplateService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> RenderAsync(string templateCode, Dictionary<string, string> templateData, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Code == templateCode, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException($"Email template with code '{templateCode}' was not found.");
        }

        var html = template.HtmlBody;

        foreach (var kvp in templateData)
        {
            html = html.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return html;
    }
}
