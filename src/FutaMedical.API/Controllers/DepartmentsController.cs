using FutaMedical.Application.Common.Models;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Departments.Queries;
using FutaMedical.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
    private readonly IApplicationDbContext _context;

    public DepartmentsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ReturnResult(ApiResponse<object>.BadRequest("Department name is required"));

        var normalizedName = request.Name.Trim().ToLower();
        var exists = await _context.Departments.AnyAsync(d => d.Name.ToLower() == normalizedName);
        if (exists)
            return ReturnResult(ApiResponse<object>.Conflict("Department already exists"));

        var department = new Department
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(new
        {
            department.Id,
            department.Name,
            department.Description,
            department.IsActive
        }));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest request)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (department == null)
            return ReturnResult(ApiResponse<object>.NotFound("Department not found"));

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedName = request.Name.Trim().ToLower();
            var duplicate = await _context.Departments.AnyAsync(d => d.Id != id && d.Name.ToLower() == normalizedName);
            if (duplicate)
                return ReturnResult(ApiResponse<object>.Conflict("Another department with this name already exists"));

            department.Name = request.Name.Trim();
        }

        if (request.Description is not null)
            department.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        department.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            department.Id,
            department.Name,
            department.Description,
            department.IsActive
        }, "Department updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var department = await _context.Departments
            .Include(d => d.Doctors)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return ReturnResult(ApiResponse<object>.NotFound("Department not found"));

        if (department.Doctors.Any())
            return ReturnResult(ApiResponse<object>.BadRequest("Cannot delete a department with assigned doctors"));

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { department.Id }, "Department deleted successfully"));
    }

    [HttpPost("{id:guid}/toggle")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleDepartment(Guid id)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (department == null)
            return ReturnResult(ApiResponse<object>.NotFound("Department not found"));

        department.IsActive = !department.IsActive;
        department.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            department.Id,
            department.IsActive
        }, department.IsActive ? "Department activated" : "Department deactivated"));
    }
}

public class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateDepartmentRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
