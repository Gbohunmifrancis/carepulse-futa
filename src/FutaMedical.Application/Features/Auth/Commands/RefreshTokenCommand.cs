using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Auth.DTOs;
using MediatR;

namespace FutaMedical.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (success, message, response) = await _identityService.RefreshTokenAsync(request.Token, request.RefreshToken);
        
        if (!success || response == null)
        {
            throw new System.UnauthorizedAccessException(message);
        }

        return response;
    }
}
