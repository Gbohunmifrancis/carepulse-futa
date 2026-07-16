using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Admin.Commands;
using FutaMedical.Application.Features.Admin.Queries;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Admin management endpoints for students and doctors
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public AdminController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Get all students
    /// </summary>
    [HttpGet("students")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentDetailDto>>), 200)]
    public async Task<IActionResult> GetAllStudents()
    {
        var result = await _mediator.Send(new GetAllStudentsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Activate a student account
    /// </summary>
    [HttpPost("students/{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        var result = await _mediator.Send(new ToggleStudentStatusCommand { StudentId = id, Activate = true });
        return ReturnResult(result);
    }

    /// <summary>
    /// Deactivate a student account
    /// </summary>
    [HttpPost("students/{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeactivateStudent(Guid id)
    {
        var result = await _mediator.Send(new ToggleStudentStatusCommand { StudentId = id, Activate = false });
        return ReturnResult(result);
    }

    /// <summary>
    /// Permanently delete a student account and all associated data
    /// </summary>
    [HttpDelete("students/{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        var result = await _mediator.Send(new DeleteStudentCommand { StudentId = id });
        return ReturnResult(result);
    }

    /// <summary>
    /// Get all doctors
    /// </summary>
    [HttpGet("doctors")]
    [ProducesResponseType(typeof(ApiResponse<List<DoctorDetailDto>>), 200)]
    public async Task<IActionResult> GetAllDoctors()
    {
        var result = await _mediator.Send(new GetAllDoctorsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Get pending doctor applications for review
    /// </summary>
    [HttpGet("doctors/pending")]
    [ProducesResponseType(typeof(ApiResponse<List<PendingDoctorApplicationDto>>), 200)]
    public async Task<IActionResult> GetPendingDoctorApplications()
    {
        var result = await _mediator.Send(new GetPendingDoctorApplicationsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Create a new doctor account and send invitation email
    /// </summary>
    [HttpPost("doctors")]
    [ProducesResponseType(typeof(ApiResponse<CreateDoctorResponseDto>), 21)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.StatusCode == StatusCodes.Status201Created && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetAllDoctors), 
                new { id = result.Data.DoctorId }, 
                result
            );
        }
        
        return ReturnResult(result);
    }

    /// <summary>
    /// Activate a doctor account
    /// </summary>
    [HttpPost("doctors/{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ActivateDoctor(Guid id)
    {
        var result = await _mediator.Send(new ToggleDoctorStatusCommand { DoctorId = id, Activate = true });
        return ReturnResult(result);
    }

    /// <summary>
    /// Deactivate a doctor account
    /// </summary>
    [HttpPost("doctors/{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeactivateDoctor(Guid id)
    {
        var result = await _mediator.Send(new ToggleDoctorStatusCommand { DoctorId = id, Activate = false });
        return ReturnResult(result);
    }

    /// <summary>
    /// Permanently delete a doctor account and all associated data
    /// </summary>
    [HttpDelete("doctors/{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteDoctor(Guid id)
    {
        var result = await _mediator.Send(new DeleteDoctorCommand { DoctorId = id });
        return ReturnResult(result);
    }

    /// <summary>
    /// Review and approve/reject a doctor's onboarding application
    /// </summary>
    /// <remarks>
    /// Admin reviews the doctor's submitted onboarding details and documents.
    /// - If approved: Doctor becomes verified and can accept appointments.
    /// - If rejected: Doctor receives rejection reason and remains unverified.
    /// 
    /// An email notification is sent to the doctor with the outcome.
    /// </remarks>
    [HttpPost("doctors/{id}/review")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ReviewDoctorApplication(Guid id, [FromBody] ReviewDoctorApplicationRequest request)
    {
        var command = new ReviewDoctorApplicationCommand
        {
            DoctorId = id,
            Approve = request.Approve,
            RejectionReason = request.RejectionReason
        };

        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var totalDoctors = await _context.Doctors.CountAsync();
        var totalStudents = await _context.Students.CountAsync();
        var pendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending");
        var totalAppointments = await _context.Appointments.CountAsync();

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            totalDoctors,
            totalStudents,
            pendingAppointments,
            totalAppointments
        }));
    }

    [HttpGet("analytics/appointments")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointmentAnalytics([FromQuery] string period = "monthly")
    {
        var normalizedPeriod = period.Trim().ToLower();
        var days = normalizedPeriod switch
        {
            "daily" => 7,
            "weekly" => 42,
            _ => 180
        };

        var fromDate = DateTime.UtcNow.Date.AddDays(-days);
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentDate >= fromDate)
            .Select(a => new { a.AppointmentDate, a.Status })
            .ToListAsync();

        var trend = appointments
            .GroupBy(a => a.AppointmentDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                date = g.Key,
                total = g.Count(),
                completed = g.Count(a => a.Status == "Completed"),
                pending = g.Count(a => a.Status == "Pending"),
                cancelled = g.Count(a => a.Status == "Cancelled")
            })
            .ToList();

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            period = normalizedPeriod,
            fromDate,
            trend
        }));
    }

    [HttpGet("analytics/departments")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentAnalytics()
    {
        var data = await _context.Departments
            .AsNoTracking()
            .Select(dept => new
            {
                departmentId = dept.Id,
                departmentName = dept.Name,
                doctorCount = dept.Doctors.Count,
                appointmentCount = dept.Doctors.SelectMany(d => d.Appointments).Count()
            })
            .OrderByDescending(x => x.appointmentCount)
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(data));
    }

    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync();
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            page,
            pageSize,
            total,
            data
        }));
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _context.SystemSettings
            .AsNoTracking()
            .OrderBy(s => s.Key)
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(settings));
    }

    [HttpPut("settings/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSetting(Guid id, [FromBody] UpdateSystemSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
            return ReturnResult(ApiResponse<object>.BadRequest("Setting value is required"));

        var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Id == id);
        if (setting == null)
            return ReturnResult(ApiResponse<object>.NotFound("Setting not found"));

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var updatedBy = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : (Guid?)null;

        setting.Value = request.Value.Trim();
        setting.Description = request.Description?.Trim();
        setting.UpdatedBy = updatedBy;
        setting.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(setting, "Setting updated successfully"));
    }

    [HttpGet("appointments")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new
            {
                a.Id,
                a.Status,
                a.AppointmentDate,
                a.StartTime,
                a.EndTime,
                a.ReasonForVisit,
                StudentName = a.Student.User.FirstName + " " + a.Student.User.LastName,
                DoctorName = a.Doctor.User.FirstName + " " + a.Doctor.User.LastName
            })
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(appointments));
    }

    [HttpPost("appointments/{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelAppointmentRequest request)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appointment == null)
            return ReturnResult(ApiResponse<object>.NotFound("Appointment not found"));

        if (appointment.Status == "Completed")
            return ReturnResult(ApiResponse<object>.BadRequest("Completed appointments cannot be cancelled"));

        appointment.Status = "Cancelled";
        appointment.CancellationReason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Cancelled by admin"
            : request.Reason.Trim();
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { appointment.Id, appointment.Status }, "Appointment cancelled successfully"));
    }

    [HttpGet("leave-requests")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] string? status)
    {
        var query = _context.DoctorLeaveRequests
            .AsNoTracking()
            .Include(l => l.Doctor)
                .ThenInclude(d => d.User)
            .OrderByDescending(l => l.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLower();
            query = query.Where(l => l.Status.ToLower() == normalizedStatus);
        }

        var leaveRequests = await query
            .Select(l => new
            {
                l.Id,
                l.DoctorId,
                DoctorName = "Dr. " + l.Doctor.User.FirstName + " " + l.Doctor.User.LastName,
                l.StartDate,
                l.EndDate,
                l.Reason,
                l.Status,
                l.AdminResponse,
                l.ReviewedAt,
                l.CreatedAt
            })
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(leaveRequests));
    }

    [HttpPost("leave-requests/{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLeaveRequest(Guid id, [FromBody] ReviewLeaveRequest request)
    {
        var leaveRequest = await _context.DoctorLeaveRequests.FirstOrDefaultAsync(l => l.Id == id);
        if (leaveRequest == null)
            return ReturnResult(ApiResponse<object>.NotFound("Leave request not found"));

        leaveRequest.Status = "Approved";
        leaveRequest.AdminResponse = string.IsNullOrWhiteSpace(request.Response) ? "Approved" : request.Response.Trim();
        leaveRequest.ReviewedAt = DateTime.UtcNow;
        leaveRequest.ReviewedByAdminId = GetUserId();
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { leaveRequest.Id, leaveRequest.Status }, "Leave request approved"));
    }

    [HttpPost("leave-requests/{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectLeaveRequest(Guid id, [FromBody] ReviewLeaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Response))
            return ReturnResult(ApiResponse<object>.BadRequest("Rejection response is required"));

        var leaveRequest = await _context.DoctorLeaveRequests.FirstOrDefaultAsync(l => l.Id == id);
        if (leaveRequest == null)
            return ReturnResult(ApiResponse<object>.NotFound("Leave request not found"));

        leaveRequest.Status = "Rejected";
        leaveRequest.AdminResponse = request.Response.Trim();
        leaveRequest.ReviewedAt = DateTime.UtcNow;
        leaveRequest.ReviewedByAdminId = GetUserId();
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { leaveRequest.Id, leaveRequest.Status }, "Leave request rejected"));
    }

    [HttpGet("health-articles")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthArticles()
    {
        var articles = await _context.HealthArticles
            .AsNoTracking()
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(articles));
    }

    [HttpPost("health-articles")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHealthArticle([FromBody] UpsertHealthArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Summary) || string.IsNullOrWhiteSpace(request.Content))
            return ReturnResult(ApiResponse<object>.BadRequest("Title, summary, and content are required"));

        var article = new FutaMedical.Domain.Entities.HealthArticle
        {
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Content = request.Content.Trim(),
            IsPublished = request.IsPublished,
            AuthorAdminId = GetUserId(),
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null
        };

        _context.HealthArticles.Add(article);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(article));
    }

    [HttpPut("health-articles/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateHealthArticle(Guid id, [FromBody] UpsertHealthArticleRequest request)
    {
        var article = await _context.HealthArticles.FirstOrDefaultAsync(h => h.Id == id);
        if (article == null)
            return ReturnResult(ApiResponse<object>.NotFound("Health article not found"));

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Summary) || string.IsNullOrWhiteSpace(request.Content))
            return ReturnResult(ApiResponse<object>.BadRequest("Title, summary, and content are required"));

        article.Title = request.Title.Trim();
        article.Summary = request.Summary.Trim();
        article.Content = request.Content.Trim();
        article.IsPublished = request.IsPublished;
        article.PublishedAt = request.IsPublished ? (article.PublishedAt ?? DateTime.UtcNow) : null;
        article.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(article, "Health article updated"));
    }

    [HttpDelete("health-articles/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHealthArticle(Guid id)
    {
        var article = await _context.HealthArticles.FirstOrDefaultAsync(h => h.Id == id);
        if (article == null)
            return ReturnResult(ApiResponse<object>.NotFound("Health article not found"));

        _context.HealthArticles.Remove(article);
        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { article.Id }, "Health article deleted"));
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
    }
}

public class ReviewDoctorApplicationRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

public class UpdateSystemSettingRequest
{
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CancelAppointmentRequest
{
    public string? Reason { get; set; }
}

public class ReviewLeaveRequest
{
    public string? Response { get; set; }
}

public class UpsertHealthArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
}
