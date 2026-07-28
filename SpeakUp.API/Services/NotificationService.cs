using SpeakUp.API.Data;
using SpeakUp.API.Models.NotificationModel;

namespace SpeakUp.API.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task CreateAsync(
        int userId,
        string title,
        string message,
        string type,
        int? reportId = null
    )
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReportId = reportId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };


        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }
}