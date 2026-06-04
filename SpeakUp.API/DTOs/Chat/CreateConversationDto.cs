using SpeakUp.API.Models.ChatModel;

namespace SpeakUp.API.DTOs.Chat
{
    public class CreateConversationDto
    {
        public ChatType ChatType { get; set; }

        public bool IsAnonymous { get; set; }

        public int? ReportId { get; set; }
    }
}