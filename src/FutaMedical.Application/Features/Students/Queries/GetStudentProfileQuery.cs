using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Students.Queries;

public record GetStudentProfileQuery : IRequest<StudentProfileDto>;

public class StudentProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string MatricNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public int YearOfStudy { get; set; }
    public string? BloodGroup { get; set; }
    public string? Genotype { get; set; }
    public string? Allergies { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetStudentProfileQueryHandler : IRequestHandler<GetStudentProfileQuery, StudentProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetStudentProfileQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<StudentProfileDto> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (student == null)
            throw new KeyNotFoundException("Student profile not found");

        return new StudentProfileDto
        {
            Id = student.Id,
            UserId = student.UserId,
            MatricNumber = student.MatricNumber,
            FirstName = student.User.FirstName,
            LastName = student.User.LastName,
            Email = student.User.Email,
            PhoneNumber = student.User.PhoneNumber,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Address = student.Address,
            Faculty = student.Faculty,
            Department = student.Department,
            YearOfStudy = student.YearOfStudy,
            BloodGroup = student.BloodGroup,
            Genotype = student.Genotype,
            Allergies = student.Allergies,
            EmergencyContactName = student.EmergencyContactName,
            EmergencyContactPhone = student.EmergencyContactPhone,
            IsVerified = student.IsVerified,
            CreatedAt = student.CreatedAt
        };
    }
}
