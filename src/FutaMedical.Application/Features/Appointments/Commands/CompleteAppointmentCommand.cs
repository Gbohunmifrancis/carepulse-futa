using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Domain.Entities;
using FutaMedical.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Appointments.Commands;

public class PrescriptionDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class VitalSignsDto
{
    public string BloodPressure { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string Pulse { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
}

public record CompleteAppointmentCommand : IRequest<ApiResponse<object>>
{
    public Guid AppointmentId { get; init; }
    public string Symptoms { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string Treatment { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public VitalSignsDto? VitalSigns { get; init; }
    public List<PrescriptionDto>? Prescriptions { get; init; }
}

public class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentCommandValidator()
    {
        RuleFor(x => x.Symptoms)
            .NotEmpty().WithMessage("Symptoms are required")
            .MaximumLength(1000).WithMessage("Symptoms cannot exceed 1000 characters");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Diagnosis is required")
            .MaximumLength(1000).WithMessage("Diagnosis cannot exceed 1000 characters");

        RuleFor(x => x.Treatment)
            .NotEmpty().WithMessage("Treatment plan is required")
            .MaximumLength(1000).WithMessage("Treatment plan cannot exceed 1000 characters");
    }
}

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CompleteAppointmentCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<object>> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor == null)
            return ApiResponse<object>.NotFound("Doctor profile not found");

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment == null)
            return ApiResponse<object>.NotFound("Appointment not found");

        if (appointment.DoctorId != doctor.Id)
            return ApiResponse<object>.BadRequest("Unauthorized: Appointment belongs to another doctor");

        if (appointment.Status != "Confirmed")
            return ApiResponse<object>.BadRequest($"Appointment must be in Confirmed state to be completed. Current status: {appointment.Status}");

        // Start database transaction
        var strategy = ((DbContext)_context).Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await ((DbContext)_context).Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Update appointment state
                appointment.Status = "Completed";
                appointment.CompletedAt = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;

                // Create medical record
                var record = new MedicalRecord
                {
                    StudentId = appointment.StudentId,
                    DoctorId = appointment.DoctorId,
                    AppointmentId = appointment.Id,
                    Symptoms = request.Symptoms,
                    Diagnosis = request.Diagnosis,
                    Treatment = request.Treatment,
                    Notes = request.Notes,
                    VitalSigns = request.VitalSigns != null ? new VitalSigns
                    {
                        BloodPressure = request.VitalSigns.BloodPressure,
                        Temperature = request.VitalSigns.Temperature,
                        Pulse = request.VitalSigns.Pulse,
                        Weight = request.VitalSigns.Weight,
                        Height = request.VitalSigns.Height
                    } : null
                };

                _context.MedicalRecords.Add(record);
                await _context.SaveChangesAsync(cancellationToken); // Generates ID for record

                // Create prescriptions if any
                if (request.Prescriptions != null && request.Prescriptions.Count > 0)
                {
                    foreach (var p in request.Prescriptions)
                    {
                        var prescription = new Prescription
                        {
                            MedicalRecordId = record.Id,
                            MedicationName = p.MedicationName,
                            Dosage = p.Dosage,
                            Frequency = p.Frequency,
                            Duration = p.Duration,
                            Instructions = p.Instructions
                        };
                        _context.Prescriptions.Add(prescription);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ApiResponse<object>.Ok(new { }, "Appointment completed and medical record generated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ApiResponse<object>.BadRequest($"An error occurred while saving appointment details: {ex.Message}");
            }
        });
    }
}
