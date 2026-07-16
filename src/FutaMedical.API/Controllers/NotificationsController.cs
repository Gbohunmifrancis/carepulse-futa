using System.Security.Claims;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutaMedical.API.Controllers;

[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public NotificationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId.Value)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.ReadAt,
                n.CreatedAt
            })
            .ToListAsync();

        return ReturnResult(ApiResponse<object>.Ok(new
        {
            page,
            pageSize,
            total,
            notifications = data
        }));
    }

    [HttpPost("{id:guid}/mark-read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value);
        if (notification == null)
            return ReturnResult(ApiResponse<object>.NotFound("Notification not found"));

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { notification.Id, notification.IsRead }, "Notification marked as read"));
    }

    [HttpPost("mark-all-read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId.Value && !n.IsRead)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
            notification.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(default);

        return ReturnResult(ApiResponse<object>.Ok(new { updated = notifications.Count }, "Notifications marked as read"));
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId == null)
            return ReturnResult(ApiResponse<object>.BadRequest("Invalid user claim"));

        var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId.Value && !n.IsRead);
        return ReturnResult(ApiResponse<object>.Ok(new { unreadCount }));
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
    }
}
