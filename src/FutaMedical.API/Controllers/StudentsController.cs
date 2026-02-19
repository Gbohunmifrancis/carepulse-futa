using System.Security.Claims;
using FutaMedical.API.Common;
using FutaMedical.Application.Features.Students.Queries;
using FutaMedical.Application.Features.Students.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Manages student profile and health information. Requires Student role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the authenticated student's full profile.
    /// </summary>
    /// <remarks>
    /// Returns the student's personal, academic, and health information including blood group, genotype, and allergies.  
    /// Requires a valid JWT token with the **Student** role.
    /// </remarks>
    /// <response code="200">Student profile returned successfully.</response>
    /// <response code="401">Unauthenticated - JWT token missing or expired.</response>
    /// <response code="403">Forbidden - user does not have the Student role.</response>
    /// <response code="404">Student profile not found for the authenticated user.</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<StudentProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<StudentProfileDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentProfileDto>>> GetProfile()
    {
        try
        {
            var result = await _mediator.Send(new GetStudentProfileQuery());
            return Ok(ApiResponse<StudentProfileDto>.SuccessResponse(result, "Success"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<StudentProfileDto>.ErrorResponse(
                ex.Message,
                new List<string> { "Profile not found" }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<StudentProfileDto>.ErrorResponse(
                "Failed to retrieve profile",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update the authenticated student's profile (mutable fields only).
    /// </summary>
    /// <remarks>
    /// **Immutable fields** (cannot be changed):  
    /// - FirstName, LastName  
    /// - MatricNumber  
    /// - DateOfBirth  
    /// - Gender  
    /// 
    /// **Mutable fields** (can be updated):  
    /// - PhoneNumber, Address  
    /// - Faculty, Department, YearOfStudy  
    /// - BloodGroup, Genotype, Allergies  
    /// - EmergencyContactName, EmergencyContactPhone  
    /// 
    /// Only provide the fields you want to update. Null/missing fields will not be changed.
    /// </remarks>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Validation failed or invalid data provided.</response>
    /// <response code="401">Unauthenticated - JWT token missing or expired.</response>
    /// <response code="403">Forbidden - user does not have the Student role.</response>
    [HttpPatch("profile")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] UpdateStudentProfileCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        
        return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
    }
}
