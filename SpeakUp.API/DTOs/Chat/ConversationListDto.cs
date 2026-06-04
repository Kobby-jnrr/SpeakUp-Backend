namespace SpeakUp.API.DTOs.Chat;

public class ConversationListDto
{
    public int Id { get; set; }

    public string ChatType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool IsAnonymous { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? LastMessage { get; set; }

    public DateTime? LastMessageTime { get; set; }
}