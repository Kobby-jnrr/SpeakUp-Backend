using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Notification;
using System.Security.Claims;

namespace SpeakUp.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;


    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/notification
    // Get logged in user's notifications
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = GetUserId();


        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReportId = n.ReportId
            })
            .ToListAsync();


        return Ok(notifications);
    }

    // GET unread count
    // Used for the red badge on the bell
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();


        var count = await _context.Notifications
            .CountAsync(n =>
                n.UserId == userId &&
                !n.IsRead
            );


        return Ok(new
        {
            unreadCount = count
        });
    }


    // Mark one notification as read
    [HttpPut("read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();


        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId
            );


        if (notification == null)
        {
            return NotFound("Notification not found.");
        }


        notification.IsRead = true;


        await _context.SaveChangesAsync();


        return Ok(new
        {
            message = "Notification marked as read."
        });
    }


    // Mark all notifications as read
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();


        var notifications = await _context.Notifications
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead
            )
            .ToListAsync();


        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }


        await _context.SaveChangesAsync();


        return Ok(new
        {
            message = "All notifications marked as read."
        });
    }

    private int GetUserId()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("User ID missing from token.");
        }


        return int.Parse(userId);
    }
}