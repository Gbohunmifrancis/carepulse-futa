using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Appointments.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Appointments.Queries;

public record GetAppointmentsQuery : IRequest<ApiResponse<List<AppointmentResponseDto>>>;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, ApiResponse<List<AppointmentResponseDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetAppointmentsQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<List<AppointmentResponseDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity!.IsAuthenticated)
            return ApiResponse<List<AppointmentResponseDto>>.BadRequest("User not authenticated");

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return ApiResponse<List<AppointmentResponseDto>>.BadRequest("Invalid user ID claim");

        var query = _context.Appointments
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor.Department)
            .AsNoTracking();

        if (user.IsInRole("Doctor"))
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
            
            if (doctor == null)
                return ApiResponse<List<AppointmentResponseDto>>.NotFound("Doctor profile not found");

            query = query.Where(a => a.DoctorId == doctor.Id);
        }
        else if (user.IsInRole("Student"))
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (student == null)
                return ApiResponse<List<AppointmentResponseDto>>.NotFound("Student profile not found");

            query = query.Where(a => a.StudentId == student.Id);
        }
        else if (!user.IsInRole("Admin"))
        {
            return ApiResponse<List<AppointmentResponseDto>>.BadRequest("Unauthorized role");
        }

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                StudentId = a.StudentId,
                StudentName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
                StudentMatricNumber = a.Student.MatricNumber,
                DoctorId = a.DoctorId,
                DoctorName = $"Dr. {a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                DoctorSpecialization = a.Doctor.Specialization ?? "",
                DepartmentName = a.Doctor.Department != null ? a.Doctor.Department.Name : "",
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime.ToString("HH:mm"),
                EndTime = a.EndTime.ToString("HH:mm"),
                Status = a.Status,
                ReasonForVisit = a.ReasonForVisit,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<AppointmentResponseDto>>.Ok(appointments);
    }
}
