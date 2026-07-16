using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Appointments.Commands;
using FutaMedical.Application.Features.Appointments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Manages appointment booking. Requires authentication.
/// </summary>
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AppointmentsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public AppointmentsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Retrieve all appointments for the logged-in user (role-dependent).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments()
    {
        var result = await _mediator.Send(new GetAppointmentsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Book a new appointment with a doctor.
    /// </summary>
    /// <remarks>
    /// Submits an appointment request to a specific doctor.  
    /// The appointment is created with **Pending** status until the doctor accepts or rejects it.  
    /// A notification is automatically sent to the doctor.  
    /// Requires a valid JWT token with the **Student** role.
    /// </remarks>
    /// <param name="command">Appointment details: doctor ID, date, start time, and reason for visit.</param>
    /// <response code="200">Appointment created successfully with Pending status.</response>
    /// <response code="401">Unauthenticated - JWT token missing or expired.</response>
    /// <response code="403">Forbidden - user does not have the Student role.</response>
    /// <response code="404">Doctor not found with the specified ID.</response>
    /// <response code="400">Validation failed or appointment conflict.</response>
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// Accept a pending appointment (Doctor only).
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptAppointment(Guid id)
    {
        var result = await _mediator.Send(new AcceptAppointmentCommand(id));
        return ReturnResult(result);
    }

    /// <summary>
    /// Reject a pending appointment (Doctor only).
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectAppointment(Guid id, [FromBody] string rejectionReason)
    {
        var result = await _mediator.Send(new RejectAppointmentCommand(id, rejectionReason));
        return ReturnResult(result);
    }

    /// <summary>
    /// Complete a confirmed appointment and generate its medical record and prescriptions (Doctor only).
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteAppointment(Guid id, [FromBody] CompleteAppointmentCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest(ApiResponse<object>.BadRequest("ID mismatch"));

        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentAppointments()
    {
        var result = await _mediator.Send(new GetAppointmentsQuery());
        return ReturnResult(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAppointmentStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
            return ReturnResult(ApiResponse<object>.NotFound("Student profile not found"));

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student.Id);
        if (appointment == null)
            return ReturnResult(ApiResponse<object>.NotFound("Appointment not found"));

        var normalizedStatus = request.Status?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedStatus) || normalizedStatus != "Cancelled")
            return ReturnResult(ApiResponse<object>.BadRequest("Only status 'Cancelled' is supported"));

        if (appointment.Status == "Completed")
            return ReturnResult(ApiResponse<object>.BadRequest("Completed appointments cannot be cancelled"));

        appointment.Status = "Cancelled";
        appointment.CancellationReason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Cancelled by student"
            : request.Reason.Trim();
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { appointment.Id, appointment.Status }, "Appointment status updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
            return ReturnResult(ApiResponse<object>.NotFound("Student profile not found"));

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student.Id);
        if (appointment == null)
            return ReturnResult(ApiResponse<object>.NotFound("Appointment not found"));

        if (appointment.Status == "Completed")
            return ReturnResult(ApiResponse<object>.BadRequest("Completed appointments cannot be deleted"));

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { id }, "Appointment deleted successfully"));
    }

    [HttpPut("{id:guid}/reschedule")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RescheduleAppointment(Guid id, [FromBody] RescheduleAppointmentRequest request)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
            return ReturnResult(ApiResponse<object>.NotFound("Student profile not found"));

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student.Id);
        if (appointment == null)
            return ReturnResult(ApiResponse<object>.NotFound("Appointment not found"));

        if (appointment.Status == "Completed")
            return ReturnResult(ApiResponse<object>.BadRequest("Completed appointments cannot be rescheduled"));

        if (request.StartTime >= request.EndTime)
            return ReturnResult(ApiResponse<object>.BadRequest("StartTime must be earlier than EndTime"));

        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.Id != appointment.Id &&
            a.DoctorId == appointment.DoctorId &&
            a.AppointmentDate.Date == request.AppointmentDate.Date &&
            a.Status != "Cancelled" &&
            a.Status != "Rejected" &&
            request.StartTime < a.EndTime &&
            request.EndTime > a.StartTime);

        if (hasConflict)
            return ReturnResult(ApiResponse<object>.Conflict("Selected slot is not available"));

        appointment.AppointmentDate = request.AppointmentDate.ToUniversalTime();
        appointment.StartTime = request.StartTime;
        appointment.EndTime = request.EndTime;
        appointment.Notes = string.IsNullOrWhiteSpace(request.Note)
            ? appointment.Notes
            : request.Note.Trim();
        appointment.Status = "Pending";
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            appointment.Id,
            appointment.AppointmentDate,
            appointment.StartTime,
            appointment.EndTime,
            appointment.Status
        }, "Appointment rescheduled successfully"));
    }

    private async Task<FutaMedical.Domain.Entities.Student?> GetCurrentStudentAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return null;

        return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}

public class UpdateAppointmentStatusRequest
{
    public string? Status { get; set; }
    public string? Reason { get; set; }
}

public class RescheduleAppointmentRequest
{
    public DateTime AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Note { get; set; }
}
