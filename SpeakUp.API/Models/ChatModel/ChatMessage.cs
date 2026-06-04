using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Models.ChatModel;

public class ChatMessage
{
    public int Id { get; set; }

    public int ChatConversationId { get; set; }
    public ChatConversation ChatConversation { get; set; } = null!;

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public required string Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}