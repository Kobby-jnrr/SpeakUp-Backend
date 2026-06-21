using SpeakUp.API.Models.ChatModel;
using SpeakUp.API.Models.ReportModel;

namespace SpeakUp.API.Models.UserModel;

public class User
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? Gender { get; set; }

    public string? Department { get; set; } = null!;

    public required string Email { get; set; }

    public required string PhoneNumber { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }

    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<ChatConversation> Conversations { get; set; } = new List<ChatConversation>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
