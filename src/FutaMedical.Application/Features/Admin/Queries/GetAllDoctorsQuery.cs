using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;

namespace FutaMedical.Application.Features.Admin.Queries;

public record GetAllDoctorsQuery : IRequest<ApiResponse<List<DoctorDetailDto>>>
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
    public string? DepartmentName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Specialization { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Qualifications { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal Rating { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public string? ApplicationStatus { get; set; }
    public DateTime? ApplicationSubmittedAt { get; set; }
    public DateTime? ApplicationReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetAllDoctorsQueryHandler : IRequestHandler<GetAllDoctorsQuery, ApiResponse<List<DoctorDetailDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<DoctorDetailDto>>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await _context.Doctors
            .Select(d => new DoctorDetailDto
            {
                Id = d.Id,
                UserId = d.UserId,
                Email = d.User.Email,
                FirstName = d.User.FirstName,
                LastName = d.User.LastName,
                PhoneNumber = d.User.PhoneNumber ?? "",
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department != null ? d.Department.Name : null,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                Qualifications = d.Qualifications,
                YearsOfExperience = d.YearsOfExperience,
                Rating = d.Rating,
                IsVerified = d.IsVerified,
                IsActive = d.User.IsActive,
                ApplicationStatus = d.ApplicationStatus,
                ApplicationSubmittedAt = d.ApplicationSubmittedAt,
                ApplicationReviewedAt = d.ApplicationReviewedAt,
                CreatedAt = d.CreatedAt
            })
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DoctorDetailDto>>.Ok(doctors);
    }
}
