using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Doctors.Commands;

public class AvailabilitySlotDto
{
    public int DayOfWeek { get; set; } // 0=Sunday, 6=Saturday
    public string StartTime { get; set; } = string.Empty; // e.g. "09:00"
    public string EndTime { get; set; } = string.Empty; // e.g. "17:00"
    public bool IsAvailable { get; set; } = true;
}

public record SetAvailabilityCommand(List<AvailabilitySlotDto> Slots) : IRequest<ApiResponse<object>>;

public class SetAvailabilityCommandValidator : AbstractValidator<SetAvailabilityCommand>
{
    public SetAvailabilityCommandValidator()
    {
        RuleFor(x => x.Slots)
            .NotEmpty().WithMessage("Availability slots cannot be empty");

        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.DayOfWeek)
                .InclusiveBetween(0, 6).WithMessage("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            
            slot.RuleFor(s => s.StartTime)
                .NotEmpty().WithMessage("Start time is required")
                .Matches(@"^(?:[01]\d|2[0-3]):[0-5]\d$").WithMessage("Start time must be in HH:mm format");

            slot.RuleFor(s => s.EndTime)
                .NotEmpty().WithMessage("End time is required")
                .Matches(@"^(?:[01]\d|2[0-3]):[0-5]\d$").WithMessage("End time must be in HH:mm format");
        });
    }
}

public class SetAvailabilityCommandHandler : IRequestHandler<SetAvailabilityCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SetAvailabilityCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<object>> Handle(SetAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor == null)
            return ApiResponse<object>.NotFound("Doctor profile not found");

        var strategy = ((DbContext)_context).Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await ((DbContext)_context).Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Remove existing availabilities
                var existing = await _context.DoctorAvailabilities
                    .Where(a => a.DoctorId == doctor.Id)
                    .ToListAsync(cancellationToken);

                _context.DoctorAvailabilities.RemoveRange(existing);

                // Add new slots
                foreach (var slot in request.Slots)
                {
                    var availability = new DoctorAvailability
                    {
                        DoctorId = doctor.Id,
                        DayOfWeek = slot.DayOfWeek,
                        StartTime = TimeOnly.Parse(slot.StartTime),
                        EndTime = TimeOnly.Parse(slot.EndTime),
                        IsAvailable = slot.IsAvailable
                    };
                    _context.DoctorAvailabilities.Add(availability);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ApiResponse<object>.Ok(new { }, "Availability slots updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ApiResponse<object>.BadRequest($"An error occurred while updating availability: {ex.Message}");
            }
        });
    }
}
