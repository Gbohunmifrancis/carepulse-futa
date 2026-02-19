using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;

namespace FutaMedical.Application.Features.Students.Commands;

/// <summary>
/// Update student profile. Immutable fields: FirstName, LastName, MatricNumber, DateOfBirth, Gender
/// </summary>
public record UpdateStudentProfileCommand : IRequest<(bool Success, string Message)>
{
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? Faculty { get; init; }
    public string? Department { get; init; }
    public int? YearOfStudy { get; init; }
    public string? BloodGroup { get; init; }
    public string? Genotype { get; init; }
    public string? Allergies { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
}

public class UpdateStudentProfileCommandValidator : AbstractValidator<UpdateStudentProfileCommand>
{
    public UpdateStudentProfileCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");
        
        RuleFor(x => x.YearOfStudy)
            .InclusiveBetween(1, 7).When(x => x.YearOfStudy.HasValue)
            .WithMessage("Year of study must be between 1 and 7");
        
        RuleFor(x => x.BloodGroup)
            .Must(bg => string.IsNullOrEmpty(bg) || new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }.Contains(bg))
            .When(x => !string.IsNullOrEmpty(x.BloodGroup))
            .WithMessage("Invalid blood group");
        
        RuleFor(x => x.Genotype)
            .Must(gt => string.IsNullOrEmpty(gt) || new[] { "AA", "AS", "SS", "AC" }.Contains(gt))
            .When(x => !string.IsNullOrEmpty(x.Genotype))
            .WithMessage("Invalid genotype");
    }
}

public class UpdateStudentProfileCommandHandler : IRequestHandler<UpdateStudentProfileCommand, (bool Success, string Message)>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateStudentProfileCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(bool Success, string Message)> Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return (false, "Unauthorized");

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (student == null)
            return (false, "Student not found");

        // Update only mutable fields
        if (request.PhoneNumber != null)
            student.User.PhoneNumber = request.PhoneNumber;

        if (request.Address != null)
            student.Address = request.Address;

        if (request.Faculty != null)
            student.Faculty = request.Faculty;

        if (request.Department != null)
            student.Department = request.Department;

        if (request.YearOfStudy.HasValue)
            student.YearOfStudy = request.YearOfStudy.Value;

        if (request.BloodGroup != null)
            student.BloodGroup = request.BloodGroup;

        if (request.Genotype != null)
            student.Genotype = request.Genotype;

        if (request.Allergies != null)
            student.Allergies = request.Allergies;

        if (request.EmergencyContactName != null)
            student.EmergencyContactName = request.EmergencyContactName;

        if (request.EmergencyContactPhone != null)
            student.EmergencyContactPhone = request.EmergencyContactPhone;

        student.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Profile updated successfully");
    }
}
