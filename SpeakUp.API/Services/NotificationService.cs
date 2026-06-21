namespace SpeakUp.API.Services;

public class NotificationService
{
    public void NotifyUser(int userId, string message)
    {
        // FUTURE:
        // - email
        // - push notification
        // - in-app notification

        Console.WriteLine($"Notify {userId}: {message}");
    }

    public void NotifyAdmin(int adminId, string message)
    {
        Console.WriteLine($"Notify Admin {adminId}: {message}");
    }
}