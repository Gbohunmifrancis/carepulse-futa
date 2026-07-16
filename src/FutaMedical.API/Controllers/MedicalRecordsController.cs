using System.Security.Claims;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.API.Controllers;

[Route("api/medical-records")]
[Authorize(Roles = "Doctor")]
[Produces("application/json")]
public class MedicalRecordsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public MedicalRecordsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordRequest request)
    {
        if (request.StudentId == Guid.Empty)
            return ReturnResult(ApiResponse<object>.BadRequest("StudentId is required"));

        if (string.IsNullOrWhiteSpace(request.Symptoms) || string.IsNullOrWhiteSpace(request.Diagnosis) || string.IsNullOrWhiteSpace(request.Treatment))
            return ReturnResult(ApiResponse<object>.BadRequest("Symptoms, diagnosis, and treatment are required"));

        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId.Value);
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var studentExists = await _context.Students.AnyAsync(s => s.Id == request.StudentId);
        if (!studentExists)
            return ReturnResult(ApiResponse<object>.NotFound("Student not found"));

        if (request.AppointmentId.HasValue)
        {
            var appointmentExists = await _context.Appointments.AnyAsync(a => a.Id == request.AppointmentId.Value && a.StudentId == request.StudentId);
            if (!appointmentExists)
                return ReturnResult(ApiResponse<object>.BadRequest("Appointment does not match selected student"));
        }

        var record = new MedicalRecord
        {
            StudentId = request.StudentId,
            DoctorId = doctor.Id,
            AppointmentId = request.AppointmentId,
            Symptoms = request.Symptoms.Trim(),
            Diagnosis = request.Diagnosis.Trim(),
            Treatment = request.Treatment.Trim(),
            Notes = request.Notes?.Trim()
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(new
        {
            record.Id,
            record.StudentId,
            record.DoctorId,
            record.AppointmentId,
            record.CreatedAt
        }, "Medical record created successfully"));
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
    }
}

public class CreateMedicalRecordRequest
{
    public Guid StudentId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
