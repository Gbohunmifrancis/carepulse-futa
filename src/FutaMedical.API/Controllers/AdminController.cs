using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Features.Admin.Commands;
using FutaMedical.Application.Features.Admin.Queries;

namespace FutaMedical.API.Controllers;

/// <summary>
/// Admin management endpoints for students and doctors
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all students
    /// </summary>
    [HttpGet("students")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentDetailDto>>), 200)]
    public async Task<IActionResult> GetAllStudents()
    {
        var result = await _mediator.Send(new GetAllStudentsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Activate a student account
    /// </summary>
    [HttpPost("students/{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        var result = await _mediator.Send(new ToggleStudentStatusCommand { StudentId = id, Activate = true });
        return ReturnResult(result);
    }

    /// <summary>
    /// Deactivate a student account
    /// </summary>
    [HttpPost("students/{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeactivateStudent(Guid id)
    {
        var result = await _mediator.Send(new ToggleStudentStatusCommand { StudentId = id, Activate = false });
        return ReturnResult(result);
    }

    /// <summary>
    /// Get all doctors
    /// </summary>
    [HttpGet("doctors")]
    [ProducesResponseType(typeof(ApiResponse<List<DoctorDetailDto>>), 200)]
    public async Task<IActionResult> GetAllDoctors()
    {
        var result = await _mediator.Send(new GetAllDoctorsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Get pending doctor applications for review
    /// </summary>
    [HttpGet("doctors/pending")]
    [ProducesResponseType(typeof(ApiResponse<List<PendingDoctorApplicationDto>>), 200)]
    public async Task<IActionResult> GetPendingDoctorApplications()
    {
        var result = await _mediator.Send(new GetPendingDoctorApplicationsQuery());
        return ReturnResult(result);
    }

    /// <summary>
    /// Create a new doctor account and send invitation email
    /// </summary>
    [HttpPost("doctors")]
    [ProducesResponseType(typeof(ApiResponse<CreateDoctorResponseDto>), 21)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.StatusCode == StatusCodes.Status201Created && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetAllDoctors), 
                new { id = result.Data.DoctorId }, 
                result
            );
        }
        
        return ReturnResult(result);
    }

    /// <summary>
    /// Activate a doctor account
    /// </summary>
    [HttpPost("doctors/{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ActivateDoctor(Guid id)
    {
        var result = await _mediator.Send(new ToggleDoctorStatusCommand { DoctorId = id, Activate = true });
        return ReturnResult(result);
    }

    /// <summary>
    /// Deactivate a doctor account
    /// </summary>
    [HttpPost("doctors/{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeactivateDoctor(Guid id)
    {
        var result = await _mediator.Send(new ToggleDoctorStatusCommand { DoctorId = id, Activate = false });
        return ReturnResult(result);
    }

    /// <summary>
    /// Review and approve/reject a doctor's onboarding application
    /// </summary>
    /// <remarks>
    /// Admin reviews the doctor's submitted onboarding details and documents.
    /// - If approved: Doctor becomes verified and can accept appointments.
    /// - If rejected: Doctor receives rejection reason and remains unverified.
    /// 
    /// An email notification is sent to the doctor with the outcome.
    /// </remarks>
    [HttpPost("doctors/{id}/review")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ReviewDoctorApplication(Guid id, [FromBody] ReviewDoctorApplicationRequest request)
    {
        var command = new ReviewDoctorApplicationCommand
        {
            DoctorId = id,
            Approve = request.Approve,
            RejectionReason = request.RejectionReason
        };

        var result = await _mediator.Send(command);
        return ReturnResult(result);
    }
}

public class ReviewDoctorApplicationRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}
