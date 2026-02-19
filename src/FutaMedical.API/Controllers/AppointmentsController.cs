using FutaMedical.API.Common;
using FutaMedical.Application.Features.Appointments.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
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
