using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Auth.DTOs;
using MediatR;

namespace FutaMedical.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (success, message, response) = await _identityService.LoginAsync(request.Email, request.Password);
        
        if (!success || response == null)
        {
            throw new System.UnauthorizedAccessException(message);
        }

        return response;
    }
}
