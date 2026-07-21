using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Domain.Entities;
using FutaMedical.Application.Features.Auth.Commands;
using FutaMedical.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Handles user authentication: registration, login, and JWT token refresh.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IEmailTemplateService _templateService;

    public AuthController(IMediator mediator, IApplicationDbContext context, IEmailTemplateService templateService)
    {
        _mediator = mediator;
        _context = context;
        _templateService = templateService;
    }

    /// <summary>
    /// Register a new student account.
    /// </summary>
    /// <remarks>
    /// Creates a new student user with health information, academic details, and emergency contact.  
    /// Returns a JWT access token and refresh token on success.
    /// </remarks>
    /// <param name="request">Student registration details including matric number, health info, and emergency contact.</param>
    /// <response code="200">Registration successful - returns JWT tokens and user profile.</response>
    /// <response code="400">Validation failed or email/matric number already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterStudentRequest request)
    {
        var result = await _mediator.Send(new RegisterStudentCommand(request));
        return ReturnResult(result);
    }

    /// <summary>
    /// Authenticate a user and obtain JWT tokens.
    /// </summary>
    /// <remarks>
    /// Validates email and password. Returns a 24-hour JWT access token and a 7-day refresh token.  
    /// Works for all roles: Admin, Doctor, Student.
    /// </remarks>
    /// <param name="request">Email and password credentials.</param>
    /// <response code="200">Login successful - returns JWT tokens and user profile.</response>
    /// <response code="401">Invalid email or password.</response>
    /// <response code="400">Request validation failed.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password));
        return ReturnResult(result);
    }

    /// <summary>
    /// Refresh an expired JWT access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// Provide the expired access token and the still-valid refresh token.  
    /// Returns a new access token and a rotated refresh token.  
    /// Refresh tokens expire after 7 days.
    /// </remarks>
    /// <param name="request">The expired JWT token and the associated refresh token.</param>
    /// <response code="200">Token refreshed - returns new JWT tokens.</response>
    /// <response code="400">Invalid or expired refresh token.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.Token, request.RefreshToken));
        return ReturnResult(result);
    }

    /// <summary>
    /// Set password for a new doctor account using setup token.
    /// </summary>
    /// <remarks>
    /// After admin creates a doctor account, the doctor receives a setup token.  
    /// Use this endpoint to set the password and basic information to activate the account.
    /// </remarks>
    /// <param name="request">Setup token, password, first name, and last name.</param>
    /// <response code="200">Password set successfully.</response>
    /// <response code="400">Invalid or expired token.</response>
    [HttpPost("set-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordCommand request)
    {
        var result = await _mediator.Send(request);
        return ReturnResult(result);
    }

    /// <summary>
    /// Get all active sessions for the currently logged-in user.
    /// </summary>
    /// <response code="200">Returns list of active sessions.</response>
    /// <response code="401">User not authenticated.</response>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<List<FutaMedical.Application.Features.Auth.Queries.UserSessionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSessions()
    {
        var result = await _mediator.Send(new FutaMedical.Application.Features.Auth.Queries.GetActiveSessionsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Logout of the current session instantly.
    /// </summary>
    /// <response code="200">Logged out successfully.</response>
    /// <response code="401">User not authenticated.</response>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var result = await _mediator.Send(new LogoutCommand());
        return ReturnResult(result);
    }

    /// <summary>
    /// Logout of all active sessions for this account.
    /// </summary>
    /// <response code="200">Logged out of all sessions successfully.</response>
    /// <response code="401">User not authenticated.</response>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("logout/all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll()
    {
        var result = await _mediator.Send(new LogoutAllCommand());
        return ReturnResult(result);
    }

    /// <summary>
    /// Logout of a specific session by token JTI.
    /// </summary>
    /// <param name="jti">The unique identifier (JTI) of the target session.</param>
    /// <response code="200">Session revoked successfully.</response>
    /// <response code="400">Session not found or already revoked.</response>
    /// <response code="401">User not authenticated.</response>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("logout/{jti}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutSession(string jti)
    {
        var result = await _mediator.Send(new LogoutSessionCommand(jti));
        return ReturnResult(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ReturnResult(ApiResponse<object>.BadRequest("Email is required"));

        var email = request.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user != null)
        {
            user.PasswordSetupToken = Guid.NewGuid().ToString("N");
            user.PasswordSetupTokenExpiry = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;

            // Build branded reset link and render HTML template
            var resetLink = $"https://campushealth.vercel.app/reset-password?token={user.PasswordSetupToken}";
            var templateVariables = new System.Collections.Generic.Dictionary<string, string>
            {
                { "ResetLink", resetLink }
            };

            string emailBody;
            string emailSubject;
            try
            {
                emailBody = await _templateService.RenderAsync("PASSWORD_RESET", templateVariables);
                var template = await _context.EmailTemplates
                    .FirstOrDefaultAsync(t => t.Code == "PASSWORD_RESET");
                emailSubject = template?.Subject ?? "FUTA Medical System - Password Reset Request";
            }
            catch
            {
                // Fallback if template not found
                emailBody = $"<p>Click the link below to reset your password:</p><a href=\"{resetLink}\">Reset Password</a><p>This link expires in 1 hour.</p>";
                emailSubject = "FUTA Medical System - Password Reset Request";
            }

            _context.EmailQueues.Add(new EmailQueue
            {
                To = user.Email,
                Subject = emailSubject,
                Body = emailBody,
                TemplateCode = "PASSWORD_RESET",
                Status = "Pending",
                Attempts = 0
            });

            await _context.SaveChangesAsync(default);
        }

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            message = "If the email exists, password reset instructions have been queued."
        }));
    }
}

/// <summary>Login credentials.</summary>
public class LoginRequest
{
    /// <summary>Registered email address.</summary>
    /// <example>student@futa.edu.ng</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Account password (min 8 chars, must include uppercase, number, and special character).</summary>
    /// <example>Student123!</example>
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
