using MediatR;
using Microsoft.EntityFrameworkCore;
using FutaMedical.Application.Common.Interfaces;

namespace FutaMedical.Application.Features.Admin.Commands;

public record ToggleDoctorStatusCommand : IRequest<(bool Success, string Message)>
{
    public Guid DoctorId { get; init; }
    public bool Activate { get; init; }
}

public class ToggleDoctorStatusCommandHandler : IRequestHandler<ToggleDoctorStatusCommand, (bool Success, string Message)>
{
    private readonly IApplicationDbContext _context;

    public ToggleDoctorStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> Handle(ToggleDoctorStatusCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor == null)
            return (false, "Doctor not found");

        doctor.User.IsActive = request.Activate;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return (true, request.Activate ? "Doctor activated successfully" : "Doctor deactivated successfully");
    }
}
