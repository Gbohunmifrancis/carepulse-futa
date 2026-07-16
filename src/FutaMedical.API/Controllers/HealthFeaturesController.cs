using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FutaMedical.API.Controllers;

[ApiController]
[Produces("application/json")]
public class HealthFeaturesController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public HealthFeaturesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/clinic-info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinicInfo()
    {
        var settings = await _context.SystemSettings
            .AsNoTracking()
            .Where(s => s.Key.StartsWith("ClinicInfo."))
            .ToListAsync();

        var info = new
        {
            phone = settings.FirstOrDefault(s => s.Key == "ClinicInfo.Phone")?.Value ?? "N/A",
            email = settings.FirstOrDefault(s => s.Key == "ClinicInfo.Email")?.Value ?? "N/A",
            address = settings.FirstOrDefault(s => s.Key == "ClinicInfo.Address")?.Value ?? "FUTA Health Centre",
            openingHours = settings.FirstOrDefault(s => s.Key == "ClinicInfo.OpeningHours")?.Value ?? "Mon-Fri, 8:00 AM - 4:00 PM",
            emergencyProcedure = settings.FirstOrDefault(s => s.Key == "ClinicInfo.EmergencyProcedure")?.Value ?? "Call clinic emergency line or visit emergency unit immediately."
        };

        return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Ok(info));
    }

    [HttpGet("api/health-resources")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthResources()
    {
        var resources = await _context.HealthArticles
            .AsNoTracking()
            .Where(h => h.IsPublished)
            .OrderByDescending(h => h.PublishedAt ?? h.CreatedAt)
            .Select(h => new
            {
                h.Id,
                h.Title,
                h.Summary,
                h.Content,
                h.PublishedAt,
                h.CreatedAt
            })
            .ToListAsync();

        return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Ok(resources));
    }

    [HttpGet("api/vaccinations")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVaccinations()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.NotFound("Student profile not found"));

        var vaccinations = await _context.VaccinationRecords
            .AsNoTracking()
            .Where(v => v.StudentId == student.Id)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync();

        return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Ok(vaccinations));
    }

    [HttpPost("api/vaccinations")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVaccination([FromBody] CreateVaccinationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VaccineName))
            return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<object>.BadRequest("VaccineName is required"));

        var student = await GetCurrentStudentAsync();
        if (student == null)
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.NotFound("Student profile not found"));

        var vaccination = new FutaMedical.Domain.Entities.VaccinationRecord
        {
            StudentId = student.Id,
            VaccineName = request.VaccineName.Trim(),
            DoseNumber = request.DoseNumber <= 0 ? 1 : request.DoseNumber,
            DateAdministered = ToUtcDateTime(request.DateAdministered),
            Provider = request.Provider?.Trim(),
            BatchNumber = request.BatchNumber?.Trim(),
            Notes = request.Notes?.Trim()
        };

        _context.VaccinationRecords.Add(vaccination);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(new
        {
            vaccination.Id,
            vaccination.StudentId,
            vaccination.VaccineName,
            vaccination.DoseNumber,
            vaccination.DateAdministered,
            vaccination.Provider,
            vaccination.BatchNumber,
            vaccination.Notes,
            vaccination.CreatedAt
        }));
    }

    [HttpGet("api/waiting-list")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWaitingList()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.NotFound("Student profile not found"));

        var entries = await _context.WaitingListEntries
            .AsNoTracking()
            .Where(w => w.StudentId == student.Id)
            .Include(w => w.Doctor)
                .ThenInclude(d => d.User)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new
            {
                w.Id,
                w.DoctorId,
                DoctorName = "Dr. " + w.Doctor.User.FirstName + " " + w.Doctor.User.LastName,
                w.PreferredDate,
                w.Reason,
                w.Status,
                w.CreatedAt
            })
            .ToListAsync();

        return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Ok(entries));
    }

    [HttpPost("api/waiting-list")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> JoinWaitingList([FromBody] CreateWaitingListEntryRequest request)
    {
        if (request.DoctorId == Guid.Empty)
            return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<object>.BadRequest("DoctorId is required"));

        var student = await GetCurrentStudentAsync();
        if (student == null)
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.NotFound("Student profile not found"));

        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == request.DoctorId);
        if (!doctorExists)
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.NotFound("Doctor not found"));

        var exists = await _context.WaitingListEntries.AnyAsync(w => w.StudentId == student.Id && w.DoctorId == request.DoctorId && w.Status == "Active");
        if (exists)
            return StatusCode(StatusCodes.Status409Conflict, ApiResponse<object>.Conflict("You already have an active waiting list entry for this doctor"));

        var entry = new FutaMedical.Domain.Entities.WaitingListEntry
        {
            StudentId = student.Id,
            DoctorId = request.DoctorId,
            PreferredDate = ToUtcDateTime(request.PreferredDate),
            Reason = request.Reason?.Trim(),
            Status = "Active"
        };

        _context.WaitingListEntries.Add(entry);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(new
        {
            entry.Id,
            entry.StudentId,
            entry.DoctorId,
            entry.PreferredDate,
            entry.Reason,
            entry.Status,
            entry.CreatedAt
        }));
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
    }

    private async Task<FutaMedical.Domain.Entities.Student?> GetCurrentStudentAsync()
    {
        var userId = GetUserId();
        if (userId == null)
            return null;

        return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId.Value);
    }

    private static DateTime ToUtcDateTime(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}

public class CreateVaccinationRequest
{
    public string VaccineName { get; set; } = string.Empty;
    public int DoseNumber { get; set; } = 1;
    public DateTime DateAdministered { get; set; }
    public string? Provider { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreateWaitingListEntryRequest
{
    public Guid DoctorId { get; set; }
    public DateTime PreferredDate { get; set; }
    public string? Reason { get; set; }
}
