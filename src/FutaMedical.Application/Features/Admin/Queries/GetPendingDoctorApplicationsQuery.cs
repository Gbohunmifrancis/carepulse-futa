using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;

namespace FutaMedical.Application.Features.Admin.Queries;

public record GetPendingDoctorApplicationsQuery : IRequest<List<PendingDoctorApplicationDto>>
{
}

public class PendingDoctorApplicationDto
{
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? Specialization { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Qualifications { get; set; }
    public int YearsOfExperience { get; set; }
    public string? Bio { get; set; }
    public string? IdentificationDocument { get; set; }
    public string? CertificateDocument { get; set; }
    public DateTime? ApplicationSubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetPendingDoctorApplicationsQueryHandler : IRequestHandler<GetPendingDoctorApplicationsQuery, List<PendingDoctorApplicationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingDoctorApplicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingDoctorApplicationDto>> Handle(GetPendingDoctorApplicationsQuery request, CancellationToken cancellationToken)
    {
        var pendingApplications = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.ApplicationStatus == "Pending")
            .Select(d => new PendingDoctorApplicationDto
            {
                DoctorId = d.Id,
                UserId = d.UserId,
                Email = d.User.Email,
                FirstName = d.User.FirstName,
                LastName = d.User.LastName,
                PhoneNumber = d.User.PhoneNumber ?? "",
                DepartmentName = d.Department != null ? d.Department.Name : null,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                Qualifications = d.Qualifications,
                YearsOfExperience = d.YearsOfExperience,
                Bio = d.Bio,
                IdentificationDocument = d.IdentificationDocument,
                CertificateDocument = d.CertificateDocument,
                ApplicationSubmittedAt = d.ApplicationSubmittedAt,
                CreatedAt = d.CreatedAt
            })
            .OrderBy(d => d.ApplicationSubmittedAt)
            .ToListAsync(cancellationToken);

        return pendingApplications;
    }
}
