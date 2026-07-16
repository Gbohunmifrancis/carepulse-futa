using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;

namespace FutaMedical.Application.Features.Admin.Commands;

public record DeleteStudentCommand : IRequest<ApiResponse<object>>
{
    public Guid StudentId { get; init; }
}

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;

    public DeleteStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

        if (student == null)
            return ApiResponse<object>.NotFound("Student not found");

        // Remove the student record — cascading deletes on appointments, records etc.
        // are enforced by FK constraints in the database schema.
        _context.Students.Remove(student);
        _context.Users.Remove(student.User);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Student account permanently deleted");
    }
}
