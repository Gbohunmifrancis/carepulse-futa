using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Appointments.Commands;

public record AcceptAppointmentCommand(Guid AppointmentId) : IRequest<ApiResponse<object>>;

public class AcceptAppointmentCommandHandler : IRequestHandler<AcceptAppointmentCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AcceptAppointmentCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<object>> Handle(AcceptAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor == null)
            return ApiResponse<object>.NotFound("Doctor profile not found");

        var appointment = await _context.Appointments
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment == null)
            return ApiResponse<object>.NotFound("Appointment not found");

        if (appointment.DoctorId != doctor.Id)
            return ApiResponse<object>.BadRequest("Unauthorized: Appointment is assigned to another doctor");

        if (appointment.Status != "Pending")
            return ApiResponse<object>.BadRequest($"Appointment cannot be accepted from its current status: {appointment.Status}");

        appointment.Status = "Confirmed";
        appointment.UpdatedAt = DateTime.UtcNow;

        var notification = new Notification
        {
            UserId = appointment.Student.UserId,
            Title = "Appointment Confirmed",
            Message = $"Your appointment request on {appointment.AppointmentDate:yyyy-MM-dd} at {appointment.StartTime:HH:mm} has been confirmed.",
            Type = "Appointment"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Appointment confirmed successfully");
    }
}
