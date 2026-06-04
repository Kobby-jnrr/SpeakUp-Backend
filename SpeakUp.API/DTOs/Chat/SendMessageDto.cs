namespace SpeakUp.API.DTOs.Chat
{
    public class SendMessageDto
    {
        public int ConversationId { get; set; }

        public string Message { get; set; } = null!;
    }
}