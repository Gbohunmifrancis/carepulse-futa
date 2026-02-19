using FutaMedical.API.Common;
using FutaMedical.Application.Features.Departments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
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
