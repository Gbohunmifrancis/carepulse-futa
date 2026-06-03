using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Prescriptions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Provides prescription management endpoints.
/// </summary>
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class PrescriptionsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PrescriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve all prescriptions for the logged-in user.
    /// </summary>
    /// <remarks>
    /// Returns prescriptions:
    /// - For Students: all prescriptions prescribed to them.
    /// - For Doctors: all prescriptions prescribed by them.
    /// - For Admins: all system prescriptions.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PrescriptionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrescriptions()
    {
        var result = await _mediator.Send(new GetPrescriptionsQuery());
        return ReturnResult(result);
    }
}
