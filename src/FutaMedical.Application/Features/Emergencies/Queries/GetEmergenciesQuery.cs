using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Emergencies.Queries;

public record GetEmergenciesQuery(string? Status = null) : IRequest<ApiResponse<List<EmergencyRequestDetailDto>>>;

public class EmergencyRequestDetailDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentMatricNumber { get; set; } = string.Empty;
    public string StudentPhoneNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? ResponseNotes { get; set; }
    public string? RespondedByName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetEmergenciesQueryHandler : IRequestHandler<GetEmergenciesQuery, ApiResponse<List<EmergencyRequestDetailDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetEmergenciesQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<List<EmergencyRequestDetailDto>>> Handle(GetEmergenciesQuery request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity!.IsAuthenticated)
            return ApiResponse<List<EmergencyRequestDetailDto>>.BadRequest("User not authenticated");

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return ApiResponse<List<EmergencyRequestDetailDto>>.BadRequest("Invalid user ID claim");

        var query = _context.EmergencyRequests
            .Include(e => e.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking();

        if (user.IsInRole("Student"))
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (student == null)
                return ApiResponse<List<EmergencyRequestDetailDto>>.NotFound("Student profile not found");

            query = query.Where(e => e.StudentId == student.Id);
        }
        else if (!user.IsInRole("Doctor") && !user.IsInRole("Admin"))
        {
            return ApiResponse<List<EmergencyRequestDetailDto>>.BadRequest("Unauthorized role");
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(e => e.Status == request.Status);
        }

        var emergencies = await query
            .OrderByDescending(e => e.Priority == "High" ? 3 : e.Priority == "Medium" ? 2 : 1)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new EmergencyRequestDetailDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = $"{e.Student.User.FirstName} {e.Student.User.LastName}",
                StudentMatricNumber = e.Student.MatricNumber,
                StudentPhoneNumber = e.Student.User.PhoneNumber ?? "",
                Description = e.Description,
                Location = e.Location,
                Status = e.Status,
                Priority = e.Priority,
                ResponseNotes = e.ResponseNotes,
                ResolvedAt = e.ResolvedAt,
                CreatedAt = e.CreatedAt,
                // We'll fetch responder name. Since RespondedBy is user Guid, we do a lookup.
                RespondedByName = e.RespondedBy.HasValue 
                    ? _context.Users.Where(u => u.Id == e.RespondedBy.Value).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() 
                    : null
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<EmergencyRequestDetailDto>>.Ok(emergencies);
    }
}
