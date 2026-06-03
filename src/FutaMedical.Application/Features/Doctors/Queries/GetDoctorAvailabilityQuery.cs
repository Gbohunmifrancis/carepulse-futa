using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Doctors.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Doctors.Queries;

public record GetDoctorAvailabilityQuery(Guid? DoctorId = null) : IRequest<ApiResponse<List<AvailabilitySlotDto>>>;

public class GetDoctorAvailabilityQueryHandler : IRequestHandler<GetDoctorAvailabilityQuery, ApiResponse<List<AvailabilitySlotDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetDoctorAvailabilityQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<List<AvailabilitySlotDto>>> Handle(GetDoctorAvailabilityQuery request, CancellationToken cancellationToken)
    {
        Guid doctorId;

        if (request.DoctorId.HasValue)
        {
            doctorId = request.DoctorId.Value;
        }
        else
        {
            var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("User not authenticated"));

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

            if (doctor == null)
                return ApiResponse<List<AvailabilitySlotDto>>.NotFound("Doctor profile not found");

            doctorId = doctor.Id;
        }

        var availabilities = await _context.DoctorAvailabilities
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .Select(a => new AvailabilitySlotDto
            {
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime.ToString("HH:mm"),
                EndTime = a.EndTime.ToString("HH:mm"),
                IsAvailable = a.IsAvailable
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<AvailabilitySlotDto>>.Ok(availabilities);
    }
}
