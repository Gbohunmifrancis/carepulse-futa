using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Emergencies.Commands;

public record ResolveEmergencyCommand(Guid EmergencyId, string ResponseNotes) : IRequest<ApiResponse<object>>;

public class ResolveEmergencyCommandValidator : AbstractValidator<ResolveEmergencyCommand>
{
    public ResolveEmergencyCommandValidator()
    {
        RuleFor(x => x.ResponseNotes)
            .NotEmpty().WithMessage("Response notes are required to resolve an emergency")
            .MaximumLength(1000).WithMessage("Response notes cannot exceed 1000 characters");
    }
}

public class ResolveEmergencyCommandHandler : IRequestHandler<ResolveEmergencyCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResolveEmergencyCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<object>> Handle(ResolveEmergencyCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var emergency = await _context.EmergencyRequests
            .FirstOrDefaultAsync(e => e.Id == request.EmergencyId, cancellationToken);

        if (emergency == null)
            return ApiResponse<object>.NotFound("Emergency request not found");

        if (emergency.Status == "Resolved")
            return ApiResponse<object>.BadRequest("Emergency is already resolved");

        emergency.Status = "Resolved";
        emergency.ResponseNotes = request.ResponseNotes;
        emergency.ResolvedAt = DateTime.UtcNow;
        emergency.UpdatedAt = DateTime.UtcNow;

        if (emergency.RespondedBy == null)
        {
            emergency.RespondedBy = userId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Emergency request resolved successfully");
    }
}
