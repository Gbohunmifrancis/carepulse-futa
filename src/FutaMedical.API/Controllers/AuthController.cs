using FutaMedical.API.Common;
using FutaMedical.Application.Features.Auth.Commands;
using FutaMedical.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Handles user authentication: registration, login, and JWT token refresh.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
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
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterStudentRequest request)
    {
        try
        {
            var result = await _mediator.Send(new RegisterStudentCommand(request));
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(
                "Registration failed", 
                new List<string> { ex.Message }));
        }
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
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _mediator.Send(new LoginCommand(request.Email, request.Password));
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse(
                ex.Message, 
                new List<string> { "Invalid credentials" }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(
                "Login failed", 
                new List<string> { ex.Message }));
        }
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
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.Token, request.RefreshToken));
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(
                "Token refresh failed", 
                new List<string> { ex.Message }));
        }
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
