using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FutaMedical.Domain.Entities;
using FutaMedical.Application.Common.Interfaces;
using BCryptLib = BCrypt.Net.BCrypt;

namespace FutaMedical.Application.Features.Admin.Commands;

public record CreateDoctorCommand : IRequest<(bool Success, string Message, Guid? DoctorId)>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid DepartmentId { get; init; }
    public string Specialization { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
    public string? Qualifications { get; init; }
    public int YearsOfExperience { get; init; }
}

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");
        
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);
    }
}

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, (bool Success, string Message, Guid? DoctorId)>
{
    private readonly IApplicationDbContext _context;

    public CreateDoctorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, Guid? DoctorId)> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return (false, "Email already exists", null);

        // Check if license number exists
        if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == request.LicenseNumber, cancellationToken))
            return (false, "License number already exists", null);

        // Check if department exists
        if (!await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId && d.IsActive, cancellationToken))
            return (false, "Department not found or inactive", null);

        // Get doctor role
        var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor", cancellationToken);
        if (doctorRole == null)
            return (false, "Doctor role not found", null);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCryptLib.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
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

        // Create doctor
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DepartmentId = request.DepartmentId,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            Qualifications = request.Qualifications,
            YearsOfExperience = request.YearsOfExperience,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Doctor created successfully", doctor.Id);
    }
}
