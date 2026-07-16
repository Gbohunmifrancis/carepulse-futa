using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using BCryptLib = BCrypt.Net.BCrypt;

namespace FutaMedical.Application.Features.Auth.Commands;

public record SetPasswordCommand : IRequest<ApiResponse<object>>
{
    public string Token { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}

public class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
{
    public SetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
    }
}

public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;

    public SetPasswordCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user by token
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordSetupToken == request.Token, cancellationToken);

        if (user == null)
            return ApiResponse<object>.BadRequest("Invalid token");

        // Check if token has expired
        if (user.PasswordSetupTokenExpiry == null || user.PasswordSetupTokenExpiry < DateTime.UtcNow)
            return ApiResponse<object>.BadRequest("Token has expired");

        // Update user with password and basic info
        user.PasswordHash = BCryptLib.HashPassword(request.Password);
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PasswordSetupToken = null;
        user.PasswordSetupTokenExpiry = null;
        user.IsActive = true; // Activate user after password is set
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Password set successfully. You can now proceed to complete your onboarding.");
    }
}
