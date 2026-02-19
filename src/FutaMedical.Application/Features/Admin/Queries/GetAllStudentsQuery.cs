using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Auth.DTOs;

namespace FutaMedical.Application.Features.Admin.Queries;

public record GetAllStudentsQuery : IRequest<List<StudentDetailDto>>
{
}

public class StudentDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MatricNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public int YearOfStudy { get; set; }
    public string? BloodGroup { get; set; }
    public string? Genotype { get; set; }
    public string? Allergies { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, List<StudentDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllStudentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentDetailDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _context.Students
            .Include(s => s.User)
            .Select(s => new StudentDetailDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Email = s.User.Email,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                PhoneNumber = s.User.PhoneNumber ?? "",
                MatricNumber = s.MatricNumber,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                Faculty = s.Faculty,
                Department = s.Department,
                YearOfStudy = s.YearOfStudy,
                BloodGroup = s.BloodGroup,
                Genotype = s.Genotype,
                Allergies = s.Allergies,
                IsVerified = s.IsVerified,
                IsActive = s.User.IsActive,
                CreatedAt = s.CreatedAt
            })
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return students;
    }
}
