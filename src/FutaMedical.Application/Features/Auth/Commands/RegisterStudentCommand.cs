using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Auth.DTOs;
using MediatR;
using FluentValidation;

namespace FutaMedical.Application.Features.Auth.Commands;

public record RegisterStudentCommand(RegisterStudentRequest Request) : IRequest<AuthResponse>;

public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    public RegisterStudentCommandValidator()
    {
        RuleFor(v => v.Request.Email)
            .NotEmpty().EmailAddress();
        
        RuleFor(v => v.Request.Password)
            .NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        
        RuleFor(v => v.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Request.MatricNumber).NotEmpty();
        // Add more validation rules as per the spec
    }
}

public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;

    public RegisterStudentCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResponse> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        var (success, message, response) = await _identityService.RegisterStudentAsync(request.Request);
        
        if (!success || response == null)
        {
            throw new System.Exception(message);
        }

        return response;
    }
}
