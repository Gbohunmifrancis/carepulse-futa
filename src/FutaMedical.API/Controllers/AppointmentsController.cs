using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Appointments.Commands;
using FutaMedical.Application.Features.Appointments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Manages appointment booking. Requires authentication.
/// </summary>
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AppointmentsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve all appointments for the logged-in user (role-dependent).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments()
    {
        var result = await _mediator.Send(new GetAppointmentsQuery());
        return ReturnResult(result);
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
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// Accept a pending appointment (Doctor only).
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptAppointment(Guid id)
    {
        var result = await _mediator.Send(new AcceptAppointmentCommand(id));
        return ReturnResult(result);
    }

    /// <summary>
    /// Reject a pending appointment (Doctor only).
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectAppointment(Guid id, [FromBody] string rejectionReason)
    {
        var result = await _mediator.Send(new RejectAppointmentCommand(id, rejectionReason));
        return ReturnResult(result);
    }

    /// <summary>
    /// Complete a confirmed appointment and generate its medical record and prescriptions (Doctor only).
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteAppointment(Guid id, [FromBody] CompleteAppointmentCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest(ApiResponse<object>.BadRequest("ID mismatch"));

        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }
}
