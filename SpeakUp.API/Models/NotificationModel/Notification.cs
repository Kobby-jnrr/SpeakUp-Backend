using SpeakUp.API.Models.ReportModel;
using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Models.NotificationModel;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public int? ReportId { get; set; }

    public Report? Report { get; set; }
}