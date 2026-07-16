using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Auth.DTOs;
using MediatR;

namespace FutaMedical.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<ApiResponse<AuthResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
    }
}
