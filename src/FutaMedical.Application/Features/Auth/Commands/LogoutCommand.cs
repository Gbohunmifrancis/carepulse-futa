using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace FutaMedical.Application.Features.Auth.Commands;

// 1. Logout Current Session Command
public record LogoutCommand : IRequest<ApiResponse<object>>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(IApplicationDbContext context, IHttpContextAccessor _accessor)
    {
        _context = context;
        _httpContextAccessor = _accessor;
    }

    public async Task<ApiResponse<object>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdString = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var jti = httpContext?.User?.FindFirst("jti")?.Value;

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId) || string.IsNullOrEmpty(jti))
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated",
                StatusCode = 401
            };
        }

        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.TokenJti == jti && !s.IsRevoked, cancellationToken);

        if (session != null)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<object>.Ok(new object(), "Logged out successfully");
    }
}

// 2. Logout All Sessions Command
public record LogoutAllCommand : IRequest<ApiResponse<object>>;

public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutAllCommandHandler(IApplicationDbContext context, IHttpContextAccessor _accessor)
    {
        _context = context;
        _httpContextAccessor = _accessor;
    }

    public async Task<ApiResponse<object>> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdString = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated",
                StatusCode = 401
            };
        }

        var activeSessions = await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
        }

        if (activeSessions.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<object>.Ok(new object(), "Logged out of all sessions successfully");
    }
}

// 3. Logout Specific Session Command
public record LogoutSessionCommand(string SessionJti) : IRequest<ApiResponse<object>>;

public class LogoutSessionCommandHandler : IRequestHandler<LogoutSessionCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutSessionCommandHandler(IApplicationDbContext context, IHttpContextAccessor _accessor)
    {
        _context = context;
        _httpContextAccessor = _accessor;
    }

    public async Task<ApiResponse<object>> Handle(LogoutSessionCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdString = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated",
                StatusCode = 401
            };
        }

        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.TokenJti == request.SessionJti && !s.IsRevoked, cancellationToken);

        if (session == null)
        {
            return ApiResponse<object>.BadRequest("Session not found or already revoked");
        }

        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new object(), $"Session '{request.SessionJti}' has been revoked successfully");
    }
}
