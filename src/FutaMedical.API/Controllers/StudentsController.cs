using System.Security.Claims;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Students.Queries;
using FutaMedical.Application.Features.Students.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Manages student profile and health information. Requires Student role.
/// </summary>
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
[Produces("application/json")]
public class StudentsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public StudentsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Get the authenticated student's full profile.
    /// </summary>
    /// <remarks>
    /// Returns the student's personal, academic, and health information including blood group, genotype, and allergies.  
    /// Requires a valid JWT token with the **Student** role.
    /// </remarks>
    /// <response code="200">Student profile returned successfully.</response>
    /// <response code="401">Unauthenticated - JWT token missing or expired.</response>
    /// <response code="403">Forbidden - user does not have the Student role.</response>
    /// <response code="404">Student profile not found for the authenticated user.</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<StudentProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<StudentProfileDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _mediator.Send(new GetStudentProfileQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Update the authenticated student's profile (mutable fields only).
    /// </summary>
    /// <remarks>
    /// **Immutable fields** (cannot be changed):  
    /// - FirstName, LastName  
    /// - MatricNumber  
    /// - DateOfBirth  
    /// - Gender  
    /// 
    /// **Mutable fields** (can be updated):  
    /// - PhoneNumber, Address  
    /// - Faculty, Department, YearOfStudy  
    /// - BloodGroup, Genotype, Allergies  
    /// - EmergencyContactName, EmergencyContactPhone  
    /// 
    /// Only provide the fields you want to update. Null/missing fields will not be changed.
    /// </remarks>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Validation failed or invalid data provided.</response>
    /// <response code="401">Unauthenticated - JWT token missing or expired.</response>
    /// <response code="403">Forbidden - user does not have the Student role.</response>
    [HttpPatch("profile")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    [HttpGet("medical-records")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedicalRecords()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null)
            return ReturnResult(ApiResponse<object>.NotFound("Student profile not found"));

        var records = await _context.MedicalRecords
            .AsNoTracking()
            .Where(m => m.StudentId == student.Id)
            .Include(m => m.Doctor)
                .ThenInclude(d => d.User)
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
                DoctorName = "Dr. " + m.Doctor.User.FirstName + " " + m.Doctor.User.LastName,
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

        return ReturnResult(ApiResponse<object>.Ok(records));
    }
}
