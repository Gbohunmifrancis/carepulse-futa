using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Appointments.Commands;

public record CreateAppointmentCommand : IRequest<AppointmentResponseDto>
{
    public Guid DoctorId { get; init; }
    public DateTime AppointmentDate { get; init; }
    public string StartTime { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentMatricNumber { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReasonForVisit { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateAppointmentCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AppointmentResponseDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (student == null)
            throw new KeyNotFoundException("Student profile not found");

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.IsVerified, cancellationToken);

        if (doctor == null)
            throw new KeyNotFoundException("Doctor not found or not verified");

        // Parse start time and calculate end time (30 minutes consultation)
        var startTime = TimeOnly.Parse(request.StartTime);
        var endTime = startTime.AddMinutes(30);

        var appointment = new Appointment
        {
            StudentId = student.Id,
            DoctorId = request.DoctorId,
            AppointmentDate = request.AppointmentDate.ToUniversalTime(),
            StartTime = startTime,
            EndTime = endTime,
            Status = "Pending",
            ReasonForVisit = request.Reason
        };

        _context.Appointments.Add(appointment);

        // Create notification for doctor
        var notification = new Notification
        {
            UserId = doctor.UserId,
            Title = "New Appointment Request",
            Message = $"New appointment request from {student.User.FirstName} {student.User.LastName}",
            Type = "Appointment"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            StudentId = student.Id,
            StudentName = $"{student.User.FirstName} {student.User.LastName}",
            StudentMatricNumber = student.MatricNumber,
            DoctorId = doctor.Id,
            DoctorName = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}",
            DoctorSpecialization = doctor.Specialization,
            DepartmentName = doctor.Department?.Name ?? "",
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime.ToString("HH:mm"),
            EndTime = appointment.EndTime.ToString("HH:mm"),
            Status = appointment.Status,
            ReasonForVisit = appointment.ReasonForVisit,
            CreatedAt = appointment.CreatedAt
        };
    }
}
