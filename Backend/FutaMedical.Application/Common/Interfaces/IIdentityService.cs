using System.Security.Claims;
using System.Threading.Tasks;
using FutaMedical.Application.Features.Auth.DTOs;

namespace FutaMedical.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Success, string Message, AuthResponse? Response)> LoginAsync(string email, string password);
    Task<(bool Success, string Message, AuthResponse? Response)> RegisterStudentAsync(RegisterStudentRequest request);
    Task<(bool Success, string Message, AuthResponse? Response)> RefreshTokenAsync(string token, string refreshToken);
}

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string firstName, string lastName, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
