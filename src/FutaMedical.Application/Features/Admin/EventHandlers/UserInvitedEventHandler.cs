using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Events;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Admin.EventHandlers;

public class UserInvitedEventHandler : INotificationHandler<UserInvitedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailTemplateService _templateService;

    public UserInvitedEventHandler(IApplicationDbContext context, IEmailTemplateService templateService)
    {
        _context = context;
        _templateService = templateService;
    }

    public async Task Handle(UserInvitedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Prepare template variables
        var setupLink = $"https://futa-medical-7ac7576e354e.herokuapp.com/setup-password?token={notification.SetupToken}";
        var variables = new Dictionary<string, string>
        {
            { "SetupLink", setupLink },
            { "ExpiresIn", "7 days" }
        };

        // 2. Render template HTML body
        var templateCode = "DOCTOR_INVITATION";
        var renderedBody = await _templateService.RenderAsync(templateCode, variables, cancellationToken);

        // 3. Fetch template to get seeded subject
        var template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Code == templateCode, cancellationToken);
        var subject = template?.Subject ?? "FUTA Medical System - Doctor Invitation";

        // 4. Create outbox queue entry
        var queueEntry = new EmailQueue
        {
            To = notification.Email,
            Subject = subject,
            Body = renderedBody,
            TemplateCode = templateCode,
            TemplateDataJson = JsonSerializer.Serialize(variables),
            Status = "Pending",
            Attempts = 0
        };

        _context.EmailQueues.Add(queueEntry);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
