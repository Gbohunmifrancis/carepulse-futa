using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Prescriptions.Queries;

public record GetPrescriptionsQuery : IRequest<ApiResponse<List<PrescriptionResponseDto>>>;

public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentMatricNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GetPrescriptionsQueryHandler : IRequestHandler<GetPrescriptionsQuery, ApiResponse<List<PrescriptionResponseDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetPrescriptionsQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<List<PrescriptionResponseDto>>> Handle(GetPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity!.IsAuthenticated)
            return ApiResponse<List<PrescriptionResponseDto>>.BadRequest("User not authenticated");

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return ApiResponse<List<PrescriptionResponseDto>>.BadRequest("Invalid user ID claim");

        var query = _context.Prescriptions
            .Include(p => p.MedicalRecord)
                .ThenInclude(m => m.Student)
                    .ThenInclude(s => s.User)
            .Include(p => p.MedicalRecord)
                .ThenInclude(m => m.Doctor)
                    .ThenInclude(d => d.User)
            .AsNoTracking();

        if (user.IsInRole("Student"))
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (student == null)
                return ApiResponse<List<PrescriptionResponseDto>>.NotFound("Student profile not found");

            query = query.Where(p => p.MedicalRecord.StudentId == student.Id);
        }
        else if (user.IsInRole("Doctor"))
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

            if (doctor == null)
                return ApiResponse<List<PrescriptionResponseDto>>.NotFound("Doctor profile not found");

            query = query.Where(p => p.MedicalRecord.DoctorId == doctor.Id);
        }
        else if (!user.IsInRole("Admin"))
        {
            return ApiResponse<List<PrescriptionResponseDto>>.BadRequest("Unauthorized role");
        }

        var prescriptions = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PrescriptionResponseDto
            {
                Id = p.Id,
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                Frequency = p.Frequency,
                Duration = p.Duration,
                Instructions = p.Instructions,
                DoctorName = $"Dr. {p.MedicalRecord.Doctor.User.FirstName} {p.MedicalRecord.Doctor.User.LastName}",
                StudentName = $"{p.MedicalRecord.Student.User.FirstName} {p.MedicalRecord.Student.User.LastName}",
                StudentMatricNumber = p.MedicalRecord.Student.MatricNumber,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<PrescriptionResponseDto>>.Ok(prescriptions);
    }
}
