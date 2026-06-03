using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FutaMedical.Domain.Entities;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using System.Security.Cryptography;

namespace FutaMedical.Application.Features.Admin.Commands;

public class CreateDoctorResponseDto
{
    public Guid DoctorId { get; set; }
    public string SetupToken { get; set; } = string.Empty;
}

public record CreateDoctorCommand : IRequest<ApiResponse<CreateDoctorResponseDto>>
{
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+234|0)[789]\d{9}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number must be a valid Nigerian phone number");
    }
}

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, ApiResponse<CreateDoctorResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public CreateDoctorCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<ApiResponse<CreateDoctorResponseDto>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return ApiResponse<CreateDoctorResponseDto>.BadRequest("Email already exists");

        // Get doctor role
        var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor", cancellationToken);
        if (doctorRole == null)
            return ApiResponse<CreateDoctorResponseDto>.BadRequest("Doctor role not found");

        // Generate password setup token
        var setupToken = GenerateSecureToken();
        var tokenExpiry = DateTime.UtcNow.AddDays(7); // Token valid for 7 days

        // Create user with temporary data
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = string.Empty, // No password yet
            FirstName = string.Empty, // Will be filled during onboarding
            LastName = string.Empty, // Will be filled during onboarding
            PhoneNumber = request.PhoneNumber,
            IsActive = false, // Inactive until password is set
            PasswordSetupToken = setupToken,
            PasswordSetupTokenExpiry = tokenExpiry,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Create user role
        _context.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = doctorRole.Id,
            AssignedAt = DateTime.UtcNow
        });

        // Create doctor profile with minimal data
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DepartmentId = null, // Will be set during onboarding
            Specialization = null,
            LicenseNumber = null,
            IsVerified = false,
            ApplicationStatus = null, // No application submitted yet
            CreatedAt = DateTime.UtcNow
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);

        // Send email with setup link containing the token
        var setupLink = $"https://futa-medical.edu.ng/setup-password?token={setupToken}";
        var emailSubject = "FUTA Medical System - Doctor Invitation";
        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                <h2>Welcome to FUTA Medical System</h2>
                <p>You have been registered as a medical practitioner on our platform.</p>
                <p>Please click the link below to set your password and complete your onboarding registration:</p>
                <p style='margin: 20px 0;'>
                    <a href='{setupLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                        Set up your account password
                    </a>
                </p>
                <p>This invitation link is valid for 7 days.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin-top: 30px;' />
                <p style='font-size: 12px; color: #777;'>Federal University of Technology, Akure Medical Center</p>
            </div>";

        try
        {
            await _emailService.SendEmailAsync(request.Email, emailSubject, emailBody);
        }
        catch
        {
            // Allow registration to proceed even if email fails, log or return token to client
        }

        var response = new CreateDoctorResponseDto
        {
            DoctorId = doctor.Id,
            SetupToken = setupToken
        };

        return ApiResponse<CreateDoctorResponseDto>.Created(response, "Doctor invitation sent successfully");
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
