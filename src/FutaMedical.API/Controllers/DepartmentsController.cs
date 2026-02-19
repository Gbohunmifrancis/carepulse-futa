using FutaMedical.API.Common;
using FutaMedical.Application.Features.Departments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Provides read access to medical departments available at the FUTA clinic.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve all active medical departments.
    /// </summary>
    /// <remarks>
    /// Returns a list of all departments accepting appointments.  
    /// This endpoint is public - no authentication required.
    /// </remarks>
    /// <response code="200">List of active departments returned successfully.</response>
    /// <response code="400">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetDepartments()
    {
        try
        {
            var result = await _mediator.Send(new GetDepartmentsQuery());
            return Ok(ApiResponse<List<DepartmentDto>>.SuccessResponse(result, "Success"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<DepartmentDto>>.ErrorResponse(
                "Failed to retrieve departments", 
                new List<string> { ex.Message }));
        }
    }
}
