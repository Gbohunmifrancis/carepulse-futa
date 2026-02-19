using System.Security.Claims;
using FutaMedical.API.Common;
using FutaMedical.Application.Features.Students.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<StudentProfileDto>>> GetProfile()
    {
        try
        {
            var result = await _mediator.Send(new GetStudentProfileQuery());
            return Ok(ApiResponse<StudentProfileDto>.SuccessResponse(result, "Success"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<StudentProfileDto>.ErrorResponse(
                ex.Message,
                new List<string> { "Profile not found" }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<StudentProfileDto>.ErrorResponse(
                "Failed to retrieve profile",
                new List<string> { ex.Message }));
        }
    }
}
