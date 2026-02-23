using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FutaMedical.API.Common;
using FutaMedical.Application.Features.Doctors.Commands;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Doctor management endpoints
/// </summary>
[Authorize(Roles = "Doctor")]
[Route("api/doctors")]
[ApiController]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Complete doctor onboarding by submitting required details and documents
    /// </summary>
    /// <remarks>
    /// After setting password, doctors must complete onboarding by providing:
    /// - Department affiliation
    /// - Specialization and license number
    /// - Qualifications and years of experience
    /// - Bio
    /// - Identification and certificate documents (URLs or file paths)
    /// 
    /// Once submitted, the application will be reviewed by an admin.
    /// </remarks>
    [HttpPost("onboarding/complete")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        
        return Ok(ApiResponse<object>.SuccessResponse(new { }, result.Message));
    }
}
