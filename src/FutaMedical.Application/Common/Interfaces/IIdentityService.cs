using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Auth.DTOs;

namespace FutaMedical.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponse>> RegisterStudentAsync(RegisterStudentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
}

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string firstName, string lastName, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
