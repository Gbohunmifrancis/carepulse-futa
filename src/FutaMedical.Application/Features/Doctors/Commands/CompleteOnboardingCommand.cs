using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FutaMedical.Application.Features.Doctors.Commands;

public record CompleteOnboardingCommand : IRequest<(bool Success, string Message)>
{
    public Guid DepartmentId { get; init; }
    public string Specialization { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
    public string? Qualifications { get; init; }
    public int YearsOfExperience { get; init; }
    public string? Bio { get; init; }
    public string? IdentificationDocument { get; init; } // URL or file path
    public string? CertificateDocument { get; init; } // URL or file path
}

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required");
        
        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required")
            .MaximumLength(200).WithMessage("Specialization must not exceed 200 characters");
        
        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required")
            .MaximumLength(50).WithMessage("License number must not exceed 50 characters");
        
        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be 0 or greater");
        
        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Bio));
        
        RuleFor(x => x.IdentificationDocument)
            .NotEmpty().WithMessage("Identification document is required");
        
        RuleFor(x => x.CertificateDocument)
            .NotEmpty().WithMessage("Certificate document is required");
    }
}

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, (bool Success, string Message)>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CompleteOnboardingCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(bool Success, string Message)> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        // Get current user ID from claims
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        // Get doctor profile
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor == null)
            return (false, "Doctor profile not found");

        // Check if onboarding already submitted
        if (doctor.ApplicationStatus != null)
            return (false, "Onboarding application has already been submitted");

        // Check if license number already exists
        if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == request.LicenseNumber && d.Id != doctor.Id, cancellationToken))
            return (false, "License number already exists");

        // Check if department exists and is active
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId && d.IsActive, cancellationToken);
        
        if (department == null)
            return (false, "Department not found or inactive");

        // Update doctor profile with onboarding data
        doctor.DepartmentId = request.DepartmentId;
        doctor.Specialization = request.Specialization;
        doctor.LicenseNumber = request.LicenseNumber;
        doctor.Qualifications = request.Qualifications;
        doctor.YearsOfExperience = request.YearsOfExperience;
        doctor.Bio = request.Bio;
        doctor.IdentificationDocument = request.IdentificationDocument;
        doctor.CertificateDocument = request.CertificateDocument;
        doctor.ApplicationStatus = "Pending";
        doctor.ApplicationSubmittedAt = DateTime.UtcNow;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Send notification to admin about new doctor application

        return (true, "Onboarding completed successfully. Your application is under review.");
    }
}
