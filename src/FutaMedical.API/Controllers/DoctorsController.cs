using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Doctors.Commands;
using FutaMedical.Application.Features.Doctors.Queries;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Doctor management endpoints
/// </summary>
[Route("api/doctors")]
public class DoctorsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public DoctorsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Complete doctor onboarding by submitting required details and documents
    /// </summary>
    [HttpPost("onboarding/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// Set or update the weekly availability slots for the logged-in doctor.
    /// </summary>
    [HttpPut("availability")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// Get the availability schedule of a doctor by doctorId.
    /// </summary>
    [HttpGet("{id:guid}/availability")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilitySlotDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilitySlotDto>>), 404)]
    public async Task<IActionResult> GetAvailability(Guid id)
    {
        var result = await _mediator.Send(new GetDoctorAvailabilityQuery(id));
        return ReturnResult(result);
    }

    /// <summary>
    /// Get the logged-in doctor's own availability schedule.
    /// </summary>
    [HttpGet("my-availability")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilitySlotDto>>), 200)]
    public async Task<IActionResult> GetMyAvailability()
    {
        var result = await _mediator.Send(new GetDoctorAvailabilityQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Get all active, verified doctors — used by students when booking an appointment.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDoctors()
    {
        var doctors = await _context.Doctors
            .AsNoTracking()
            .Where(d => d.IsVerified && d.User.IsActive)
            .Include(d => d.User)
            .Include(d => d.Department)
            .OrderBy(d => d.User.LastName)
            .Select(d => new
            {
                d.Id,
                FullName = "Dr. " + d.User.FirstName + " " + d.User.LastName,
                d.Specialization,
                d.Qualifications,
                d.YearsOfExperience,
                d.Bio,
                d.Rating,
                d.TotalReviews,
                Department = d.Department != null ? d.Department.Name : null,
                DepartmentId = d.DepartmentId
            })
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(doctors));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDoctorById(Guid id)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor not found"));

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            doctor.Id,
            doctor.Specialization,
            doctor.Qualifications,
            doctor.YearsOfExperience,
            doctor.Bio,
            doctor.Rating,
            doctor.TotalReviews,
            FullName = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}",
            Department = doctor.Department?.Name,
            doctor.IsVerified
        }));
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDoctorProfile()
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var doctor = await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId.Value);

        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            doctor.Id,
            doctor.Specialization,
            doctor.Qualifications,
            doctor.YearsOfExperience,
            doctor.Bio,
            doctor.LicenseNumber,
            doctor.IsVerified,
            doctor.ApplicationStatus,
            doctor.Rating,
            doctor.TotalReviews,
            Department = doctor.Department?.Name,
            User = new
            {
                doctor.User.Email,
                doctor.User.FirstName,
                doctor.User.LastName,
                doctor.User.PhoneNumber
            }
        }));
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDoctorProfile([FromBody] UpdateDoctorProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId.Value);

        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        if (request.Bio is not null) doctor.Bio = request.Bio.Trim();
        if (request.Specialization is not null) doctor.Specialization = request.Specialization.Trim();
        if (request.Qualifications is not null) doctor.Qualifications = request.Qualifications.Trim();
        if (request.YearsOfExperience.HasValue) doctor.YearsOfExperience = Math.Max(0, request.YearsOfExperience.Value);
        if (request.PhoneNumber is not null) doctor.User.PhoneNumber = request.PhoneNumber.Trim();

        doctor.UpdatedAt = DateTime.UtcNow;
        doctor.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { doctor.Id }, "Doctor profile updated successfully"));
    }

    [HttpGet("patients/{studentId:guid}/history")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientHistory(Guid studentId)
    {
        var records = await _context.MedicalRecords
            .AsNoTracking()
            .Where(m => m.StudentId == studentId)
            .Include(m => m.Prescriptions)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.AppointmentId,
                m.Symptoms,
                m.Diagnosis,
                m.Treatment,
                m.Notes,
                m.VitalSigns,
                m.CreatedAt,
                Prescriptions = m.Prescriptions.Select(p => new
                {
                    p.Id,
                    p.MedicationName,
                    p.Dosage,
                    p.Frequency,
                    p.Duration,
                    p.Instructions
                })
            })
            .ToListAsync();

        if (records.Count == 0)
            return ReturnResult(ApiResponse<object>.NotFound("No medical history found for this student"));

        return ReturnResult(ApiResponse<object>.Ok(records));
    }

    [HttpGet("dashboard/stats")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDoctorDashboardStats()
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId.Value);
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var today = DateTime.UtcNow.Date;

        var totalPatientsSeen = await _context.Appointments
            .Where(a => a.DoctorId == doctor.Id && a.Status == "Completed")
            .Select(a => a.StudentId)
            .Distinct()
            .CountAsync();

        var upcomingToday = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today && (a.Status == "Pending" || a.Status == "Confirmed"));

        var pendingAppointments = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctor.Id && a.Status == "Pending");

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            totalPatientsSeen,
            upcomingToday,
            pendingAppointments,
            averageRating = doctor.Rating,
            totalReviews = doctor.TotalReviews
        }));
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
    }

    [HttpGet("leave-requests")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyLeaveRequests()
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId.Value);
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var leaveRequests = await _context.DoctorLeaveRequests
            .AsNoTracking()
            .Where(l => l.DoctorId == doctor.Id)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(leaveRequests));
    }

    [HttpPost("leave-requests")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitLeaveRequest([FromBody] SubmitLeaveRequestRequest request)
    {
        if (request.StartDate.Date > request.EndDate.Date)
            return ReturnResult(ApiResponse<object>.BadRequest("StartDate cannot be after EndDate"));

        if (string.IsNullOrWhiteSpace(request.Reason))
            return ReturnResult(ApiResponse<object>.BadRequest("Reason is required"));

        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId.Value);
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var leaveRequest = new FutaMedical.Domain.Entities.DoctorLeaveRequest
        {
            DoctorId = doctor.Id,
            StartDate = ToUtcDate(request.StartDate),
            EndDate = ToUtcDate(request.EndDate),
            Reason = request.Reason.Trim(),
            Status = "Pending"
        };

        _context.DoctorLeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(leaveRequest));
    }

    [HttpGet("prescription-templates")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrescriptionTemplates()
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var templates = await _context.PrescriptionTemplates
            .AsNoTracking()
            .Where(t => t.DoctorId == doctor.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(templates));
    }

    [HttpPost("prescription-templates")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePrescriptionTemplate([FromBody] UpsertPrescriptionTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.MedicationName) || string.IsNullOrWhiteSpace(request.Dosage) || string.IsNullOrWhiteSpace(request.Frequency) || string.IsNullOrWhiteSpace(request.Duration))
            return ReturnResult(ApiResponse<object>.BadRequest("Name, medicationName, dosage, frequency, and duration are required"));

        var doctor = await GetCurrentDoctorAsync();
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var template = new FutaMedical.Domain.Entities.PrescriptionTemplate
        {
            DoctorId = doctor.Id,
            Name = request.Name.Trim(),
            MedicationName = request.MedicationName.Trim(),
            Dosage = request.Dosage.Trim(),
            Frequency = request.Frequency.Trim(),
            Duration = request.Duration.Trim(),
            Instructions = request.Instructions?.Trim()
        };

        _context.PrescriptionTemplates.Add(template);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(new
        {
            template.Id,
            template.DoctorId,
            template.Name,
            template.MedicationName,
            template.Dosage,
            template.Frequency,
            template.Duration,
            template.Instructions,
            template.CreatedAt
        }));
    }

    [HttpPost("referrals")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReferral([FromBody] CreateReferralRequest request)
    {
        if (request.StudentId == Guid.Empty || string.IsNullOrWhiteSpace(request.HospitalName) || string.IsNullOrWhiteSpace(request.Reason))
            return ReturnResult(ApiResponse<object>.BadRequest("StudentId, hospitalName, and reason are required"));

        var doctor = await GetCurrentDoctorAsync();
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var studentExists = await _context.Students.AnyAsync(s => s.Id == request.StudentId);
        if (!studentExists)
            return ReturnResult(ApiResponse<object>.NotFound("Student not found"));

        var referral = new FutaMedical.Domain.Entities.Referral
        {
            DoctorId = doctor.Id,
            StudentId = request.StudentId,
            AppointmentId = request.AppointmentId,
            HospitalName = request.HospitalName.Trim(),
            Reason = request.Reason.Trim(),
            Notes = request.Notes?.Trim()
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(referral));
    }

    /// <summary>
    /// Search a student's medical history by name or matric number.
    /// </summary>
    [HttpGet("patients/search")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchPatients([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return ReturnResult(ApiResponse<object>.BadRequest("Search term is required"));

        var normalizedTerm = term.Trim().ToLower();

        var patients = await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s =>
                s.MatricNumber.ToLower().Contains(normalizedTerm) ||
                s.User.FirstName.ToLower().Contains(normalizedTerm) ||
                s.User.LastName.ToLower().Contains(normalizedTerm) ||
                (s.User.FirstName + " " + s.User.LastName).ToLower().Contains(normalizedTerm))
            .OrderBy(s => s.User.LastName)
            .Select(s => new
            {
                s.Id,
                FullName = s.User.FirstName + " " + s.User.LastName,
                s.MatricNumber,
                s.Faculty,
                s.Department,
                s.BloodGroup,
                s.Genotype,
                Email = s.User.Email
            })
            .Take(50)
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(patients));
    }

    /// <summary>
    /// Get the logged-in doctor's own ratings and reviews.
    /// </summary>
    [HttpGet("reviews")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyReviews()
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var reviews = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.DoctorId == doctor.Id)
            .Include(r => r.Student)
                .ThenInclude(s => s.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.Response,
                r.RespondedAt,
                r.CreatedAt,
                StudentName = r.Student.User.FirstName + " " + r.Student.User.LastName,
                r.AppointmentId
            })
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            averageRating = doctor.Rating,
            totalReviews = doctor.TotalReviews,
            reviews
        }));
    }

    private async Task<FutaMedical.Domain.Entities.Doctor?> GetCurrentDoctorAsync()
    {
        var userId = GetUserId();
        if (userId == null)
            return null;

        return await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId.Value);
    }

    private static DateTime ToUtcDate(DateTime value)
    {
        var date = value.Date;
        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
    }
}

public class UpdateDoctorProfileRequest
{
    public string? Bio { get; set; }
    public string? Qualifications { get; set; }
    public string? Specialization { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? PhoneNumber { get; set; }
}

public class SubmitLeaveRequestRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class UpsertPrescriptionTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class CreateReferralRequest
{
    public Guid StudentId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
