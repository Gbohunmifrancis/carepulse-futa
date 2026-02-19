using FutaMedical.API.Common;
using FutaMedical.Application.Features.Auth.Commands;
using FutaMedical.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
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

    [HttpPost("login")]
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

    [HttpPost("refresh-token")]
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

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
