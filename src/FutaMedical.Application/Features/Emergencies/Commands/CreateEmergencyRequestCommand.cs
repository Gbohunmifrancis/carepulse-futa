using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.Application.Features.Emergencies.Commands;

public record CreateEmergencyRequestCommand : IRequest<ApiResponse<EmergencyRequestResponseDto>>
{
    public string Description { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Priority { get; init; } = "Low"; // Low, Medium, High
}

public class CreateEmergencyRequestCommandValidator : AbstractValidator<CreateEmergencyRequestCommand>
{
    public CreateEmergencyRequestCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Emergency description is required")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location cannot exceed 500 characters");

        RuleFor(x => x.Priority)
            .Must(p => new[] { "Low", "Medium", "High" }.Contains(p))
            .WithMessage("Priority must be Low, Medium, or High");
    }
}

public class EmergencyRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentMatricNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateEmergencyRequestCommandHandler : IRequestHandler<CreateEmergencyRequestCommand, ApiResponse<EmergencyRequestResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateEmergencyRequestCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<EmergencyRequestResponseDto>> Handle(CreateEmergencyRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User not authenticated"));

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (student == null)
            return ApiResponse<EmergencyRequestResponseDto>.NotFound("Student profile not found");

        var emergency = new EmergencyRequest
        {
            StudentId = student.Id,
            Description = request.Description,
            Location = request.Location,
            Priority = request.Priority,
            Status = "Pending"
        };

        _context.EmergencyRequests.Add(emergency);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Trigger real-time notification alert to clinic staff

        var dto = new EmergencyRequestResponseDto
        {
            Id = emergency.Id,
            StudentId = student.Id,
            StudentName = $"{student.User.FirstName} {student.User.LastName}",
            StudentMatricNumber = student.MatricNumber,
            Description = emergency.Description,
            Location = emergency.Location,
            Status = emergency.Status,
            Priority = emergency.Priority,
            CreatedAt = emergency.CreatedAt
        };

        return ApiResponse<EmergencyRequestResponseDto>.Created(dto, "Emergency request logged successfully");
    }
}
