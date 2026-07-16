using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Domain.Entities;

namespace FutaMedical.Application.Features.Admin.Commands;

public record ReviewDoctorApplicationCommand : IRequest<ApiResponse<object>>
{
    public Guid DoctorId { get; init; }
    public bool Approve { get; init; }
    public string? RejectionReason { get; init; }
}

public class ReviewDoctorApplicationCommandValidator : AbstractValidator<ReviewDoctorApplicationCommand>
{
    public ReviewDoctorApplicationCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required");
        
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required when rejecting an application")
            .When(x => !x.Approve);
    }
}

public class ReviewDoctorApplicationCommandHandler : IRequestHandler<ReviewDoctorApplicationCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;

    public ReviewDoctorApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> Handle(ReviewDoctorApplicationCommand request, CancellationToken cancellationToken)
    {
        // Get doctor with user details
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor == null)
            return ApiResponse<object>.NotFound("Doctor not found");

        // Check if application is pending
        if (doctor.ApplicationStatus != "Pending")
            return ApiResponse<object>.BadRequest($"Application is not pending. Current status: {doctor.ApplicationStatus ?? "Not submitted"}");

        // Update application status
        if (request.Approve)
        {
            doctor.ApplicationStatus = "Approved";
            doctor.IsVerified = true;
            doctor.ApplicationReviewedAt = DateTime.UtcNow;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // TODO: Send approval email to doctor
            // Email content: "Your application has been approved. You can now start accepting appointments."

            return ApiResponse<object>.Ok(new { }, "Doctor application approved successfully");
        }
        else
        {
            doctor.ApplicationStatus = "Rejected";
            doctor.IsVerified = false;
            doctor.ApplicationReviewedAt = DateTime.UtcNow;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // TODO: Send rejection email to doctor with reason
            // Email content: $"Your application has been rejected. Reason: {request.RejectionReason}"

            return ApiResponse<object>.Ok(new { }, "Doctor application rejected successfully");
        }
    }
}
