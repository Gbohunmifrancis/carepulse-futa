using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;

namespace FutaMedical.Application.Features.Admin.Commands;

public record DeleteDoctorCommand : IRequest<ApiResponse<object>>
{
    public Guid DoctorId { get; init; }
}

public class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;

    public DeleteDoctorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor == null)
            return ApiResponse<object>.NotFound("Doctor not found");

        // Remove the doctor record — cascading deletes on appointments, reviews etc.
        // are enforced by FK constraints in the database schema.
        _context.Doctors.Remove(doctor);
        _context.Users.Remove(doctor.User);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Doctor account permanently deleted");
    }
}
