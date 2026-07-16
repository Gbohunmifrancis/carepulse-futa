using System.Security.Claims;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using FutaMedical.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReviewsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReviewsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("{appointmentId:guid}")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitReview(Guid appointmentId, [FromBody] SubmitReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return ReturnResult(ApiResponse<object>.BadRequest("Rating must be between 1 and 5"));

        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student == null)
            return ReturnResult(ApiResponse<object>.NotFound("Student profile not found"));

        var appointment = await _context.Appointments
            .Include(a => a.Review)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.StudentId == student.Id);

        if (appointment == null)
            return ReturnResult(ApiResponse<object>.NotFound("Appointment not found"));

        if (appointment.Status != "Completed")
            return ReturnResult(ApiResponse<object>.BadRequest("Review can only be submitted for completed appointments"));

        if (appointment.Review != null)
            return ReturnResult(ApiResponse<object>.Conflict("A review already exists for this appointment"));

        var review = new Review
        {
            StudentId = appointment.StudentId,
            DoctorId = appointment.DoctorId,
            AppointmentId = appointment.Id,
            Rating = request.Rating,
            Comment = request.Comment?.Trim()
        };

        _context.Reviews.Add(review);

        var doctorReviews = await _context.Reviews
            .Where(r => r.DoctorId == appointment.DoctorId)
            .Select(r => r.Rating)
            .ToListAsync();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == appointment.DoctorId);
        if (doctor != null)
        {
            var allRatings = doctorReviews.Append(request.Rating).ToList();
            doctor.TotalReviews = allRatings.Count;
            doctor.Rating = Math.Round((decimal)allRatings.Average(), 1);
            doctor.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(default);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Created(new
        {
            review.Id,
            review.Rating,
            review.Comment
        }, "Review submitted successfully"));
    }

    [HttpPost("{id:guid}/respond")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RespondToReview(Guid id, [FromBody] RespondToReviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Response))
            return ReturnResult(ApiResponse<object>.BadRequest("Response is required"));

        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId.Value);
        if (doctor == null)
            return ReturnResult(ApiResponse<object>.NotFound("Doctor profile not found"));

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.DoctorId == doctor.Id);
        if (review == null)
            return ReturnResult(ApiResponse<object>.NotFound("Review not found"));

        review.Response = request.Response.Trim();
        review.RespondedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { review.Id, review.Response, review.RespondedAt }, "Review response saved"));
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
    }
}

public class SubmitReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class RespondToReviewRequest
{
    public string Response { get; set; } = string.Empty;
}
