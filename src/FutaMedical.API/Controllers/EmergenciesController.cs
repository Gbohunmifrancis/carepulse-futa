using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Emergencies.Commands;
using FutaMedical.Application.Features.Emergencies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Provides endpoints for logging and handling medical emergencies.
/// </summary>
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class EmergenciesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public EmergenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Log a new emergency request (Student only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyRequestResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmergencyRequest([FromBody] CreateEmergencyRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// Retrieve list of emergency requests (role-dependent filtering).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Student,Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<EmergencyRequestDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmergencies([FromQuery] string? status)
    {
        var result = await _mediator.Send(new GetEmergenciesQuery(status));
        return ReturnResult(result);
    }

    /// <summary>
    /// Respond to an active emergency request (Doctor/Admin only).
    /// </summary>
    [HttpPost("{id:guid}/respond")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RespondToEmergency(Guid id)
    {
        var result = await _mediator.Send(new RespondToEmergencyCommand(id));
        return ReturnResult(result);
    }

    /// <summary>
    /// Resolve an emergency request (Doctor/Admin only).
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResolveEmergency(Guid id, [FromBody] string responseNotes)
    {
        var result = await _mediator.Send(new ResolveEmergencyCommand(id, responseNotes));
        return ReturnResult(result);
    }
}
