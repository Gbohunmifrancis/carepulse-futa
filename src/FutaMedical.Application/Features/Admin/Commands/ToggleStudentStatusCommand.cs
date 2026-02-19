using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;

namespace FutaMedical.Application.Features.Admin.Commands;

public record ToggleStudentStatusCommand : IRequest<(bool Success, string Message)>
{
    public Guid StudentId { get; init; }
    public bool Activate { get; init; } // true = activate, false = deactivate
}

public class ToggleStudentStatusCommandHandler : IRequestHandler<ToggleStudentStatusCommand, (bool Success, string Message)>
{
    private readonly IApplicationDbContext _context;

    public ToggleStudentStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> Handle(ToggleStudentStatusCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

        if (student == null)
            return (false, "Student not found");

        student.User.IsActive = request.Activate;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return (true, request.Activate ? "Student activated successfully" : "Student deactivated successfully");
    }
}
