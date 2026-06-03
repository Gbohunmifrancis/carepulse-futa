using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Doctors.Commands;
using FutaMedical.Application.Features.Doctors.Queries;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Doctor management endpoints
/// </summary>
[Route("api/doctors")]
public class DoctorsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Complete doctor onboarding by submitting required details and documents
    /// </summary>
    [HttpPost("onboarding/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// Set or update the weekly availability slots for the logged-in doctor.
    /// </summary>
    [HttpPut("availability")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// Get the availability schedule of a doctor by doctorId.
    /// </summary>
    [HttpGet("{id:guid}/availability")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilitySlotDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilitySlotDto>>), 404)]
    public async Task<IActionResult> GetAvailability(Guid id)
    {
        var result = await _mediator.Send(new GetDoctorAvailabilityQuery(id));
        return ReturnResult(result);
    }

    /// <summary>
    /// Get the logged-in doctor's own availability schedule.
    /// </summary>
    [HttpGet("my-availability")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilitySlotDto>>), 200)]
    public async Task<IActionResult> GetMyAvailability()
    {
        var result = await _mediator.Send(new GetDoctorAvailabilityQuery());
        return ReturnResult(result);
    }
}
