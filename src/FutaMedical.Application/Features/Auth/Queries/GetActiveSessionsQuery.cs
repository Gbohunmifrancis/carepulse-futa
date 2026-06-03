using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace FutaMedical.Application.Features.Auth.Queries;

public class UserSessionDto
{
    public Guid Id { get; set; }
    public string TokenJti { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCurrentSession { get; set; }
}

public record GetActiveSessionsQuery : IRequest<ApiResponse<List<UserSessionDto>>>;

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, ApiResponse<List<UserSessionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetActiveSessionsQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<List<UserSessionDto>>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdString = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return new ApiResponse<List<UserSessionDto>>
            {
                Success = false,
                Message = "User is not authenticated",
                StatusCode = 401
            };
        }

        var currentJti = httpContext?.User?.FindFirst("jti")?.Value;

        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new UserSessionDto
            {
                Id = s.Id,
                TokenJti = s.TokenJti,
                UserAgent = s.UserAgent,
                IpAddress = s.IpAddress,
                CreatedAt = s.CreatedAt,
                ExpiresAt = s.ExpiresAt,
                IsCurrentSession = s.TokenJti == currentJti
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<UserSessionDto>>.Ok(sessions, "Active sessions retrieved successfully");
    }
}
