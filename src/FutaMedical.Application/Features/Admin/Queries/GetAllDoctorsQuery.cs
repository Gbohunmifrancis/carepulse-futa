using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;

namespace FutaMedical.Application.Features.Admin.Queries;

public record GetAllDoctorsQuery : IRequest<List<DoctorDetailDto>>
{
}

public class DoctorDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Qualifications { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal Rating { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetAllDoctorsQueryHandler : IRequestHandler<GetAllDoctorsQuery, List<DoctorDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorDetailDto>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .Select(d => new DoctorDetailDto
            {
                Id = d.Id,
                UserId = d.UserId,
                Email = d.User.Email,
                FirstName = d.User.FirstName,
                LastName = d.User.LastName,
                PhoneNumber = d.User.PhoneNumber ?? "",
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department.Name,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                Qualifications = d.Qualifications,
                YearsOfExperience = d.YearsOfExperience,
                Rating = d.Rating,
                IsVerified = d.IsVerified,
                IsActive = d.User.IsActive,
                CreatedAt = d.CreatedAt
            })
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return doctors;
    }
}
