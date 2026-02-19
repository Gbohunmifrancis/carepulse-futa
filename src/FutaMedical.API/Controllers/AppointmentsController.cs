using FutaMedical.API.Common;
using FutaMedical.Application.Features.Appointments.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Manages appointment booking. Requires authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Book a new appointment with a doctor.
    /// </summary>
    /// <remarks>
    /// Submits an appointment request to a specific doctor.  
    /// The appointment is created with **Pending** status until the doctor accepts or rejects it.  
    /// A notification is automatically sent to the doctor.  
    /// Requires a valid JWT token with the **Student** role.
    /// </remarks>
    /// <param name="command">Appointment details: doctor ID, date, start time, and reason for visit.</param>
    /// <response code="200">Appointment created successfully with Pending status.</response>
    /// <response code="401">Unauthenticated - JWT token missing or expired.</response>
    /// <response code="403">Forbidden - user does not have the Student role.</response>
    /// <response code="404">Doctor not found with the specified ID.</response>
    /// <response code="400">Validation failed or appointment conflict.</response>
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> CreateAppointment([FromBody] CreateAppointmentCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment request submitted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AppointmentResponseDto>.ErrorResponse(
                ex.Message,
                new List<string> { "Resource not found" }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse(
                "Failed to create appointment",
                new List<string> { ex.Message }));
        }
    }
}
