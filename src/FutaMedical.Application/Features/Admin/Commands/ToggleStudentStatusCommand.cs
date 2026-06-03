using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;

namespace FutaMedical.Application.Features.Admin.Commands;

public record ToggleStudentStatusCommand : IRequest<ApiResponse<object>>
{
    public Guid StudentId { get; init; }
    public bool Activate { get; init; } // true = activate, false = deactivate
}

public class ToggleStudentStatusCommandHandler : IRequestHandler<ToggleStudentStatusCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;

    public ToggleStudentStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> Handle(ToggleStudentStatusCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

        if (student == null)
            return ApiResponse<object>.NotFound("Student not found");

        student.User.IsActive = request.Activate;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, request.Activate ? "Student activated successfully" : "Student deactivated successfully");
    }
}
