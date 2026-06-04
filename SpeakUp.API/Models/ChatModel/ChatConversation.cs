using SpeakUp.API.Models.ReportModel;
using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Models.ChatModel;

public class ChatConversation
{
    public int Id { get; set; }

    public ChatType ChatType { get; set; }

    public ConversationStatus Status { get; set; } = ConversationStatus.Open;

    public bool IsAnonymous { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public int? AssignedAdminId { get; set; }
    public User? AssignedAdmin { get; set; }

    public int? PreviousAdminId { get; set; }
    public User? PreviousAdmin { get; set; }

    public int? ReportId { get; set; }
    public Report? Report { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }
}