using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Emergencies.Commands;

public record RespondToEmergencyCommand(Guid EmergencyId) : IRequest<ApiResponse<object>>;

public class RespondToEmergencyCommandHandler : IRequestHandler<RespondToEmergencyCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RespondToEmergencyCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<object>> Handle(RespondToEmergencyCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var emergency = await _context.EmergencyRequests
            .FirstOrDefaultAsync(e => e.Id == request.EmergencyId, cancellationToken);

        if (emergency == null)
            return ApiResponse<object>.NotFound("Emergency request not found");

        if (emergency.Status != "Pending")
            return ApiResponse<object>.BadRequest($"Emergency is already being handled. Current status: {emergency.Status}");

        emergency.Status = "InProgress";
        emergency.RespondedBy = userId;
        emergency.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Emergency marked as In Progress");
    }
}
