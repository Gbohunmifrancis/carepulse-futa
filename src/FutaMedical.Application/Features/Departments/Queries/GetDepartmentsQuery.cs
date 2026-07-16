using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Departments.Queries;

public record GetDepartmentsQuery : IRequest<ApiResponse<List<DepartmentDto>>>;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, ApiResponse<List<DepartmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<DepartmentDto>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DepartmentDto>>.Ok(departments);
    }
}
