using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;

namespace FutaMedical.Application.Features.Admin.Commands;

public record ToggleDoctorStatusCommand : IRequest<ApiResponse<object>>
{
    public Guid DoctorId { get; init; }
    public bool Activate { get; init; }
}

public class ToggleDoctorStatusCommandHandler : IRequestHandler<ToggleDoctorStatusCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;

    public ToggleDoctorStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> Handle(ToggleDoctorStatusCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor == null)
            return ApiResponse<object>.NotFound("Doctor not found");

        doctor.User.IsActive = request.Activate;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, request.Activate ? "Doctor activated successfully" : "Doctor deactivated successfully");
    }
}
