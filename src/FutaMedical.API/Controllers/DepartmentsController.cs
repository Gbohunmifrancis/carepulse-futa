using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Departments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Provides read access to medical departments available at the FUTA clinic.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ApiControllerBase
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
    public async Task<IActionResult> GetDepartments()
    {
        var result = await _mediator.Send(new GetDepartmentsQuery());
        return ReturnResult(result);
    }
}
