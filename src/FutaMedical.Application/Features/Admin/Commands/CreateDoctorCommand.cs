using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FutaMedical.Domain.Entities;
using FutaMedical.Application.Common.Interfaces;
using System.Security.Cryptography;

namespace FutaMedical.Application.Features.Admin.Commands;

public record CreateDoctorCommand : IRequest<(bool Success, string Message, Guid? DoctorId, string? SetupToken)>
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

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, (bool Success, string Message, Guid? DoctorId, string? SetupToken)>
{
    private readonly IApplicationDbContext _context;

    public CreateDoctorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, Guid? DoctorId, string? SetupToken)> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return (false, "Email already exists", null, null);

        // Get doctor role
        var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor", cancellationToken);
        if (doctorRole == null)
            return (false, "Doctor role not found", null, null);

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

        // TODO: Send email with setup link containing the token
        // Example: https://yourapp.com/setup-password?token={setupToken}

        return (true, "Doctor invitation sent successfully", doctor.Id, setupToken);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
