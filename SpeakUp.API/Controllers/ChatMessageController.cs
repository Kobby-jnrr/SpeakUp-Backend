using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Chat;
using SpeakUp.API.Models.ChatModel;
using System.Security.Claims;

namespace SpeakUp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatMessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                return Unauthorized();

            var userId = int.Parse(claim.Value);

            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == dto.ConversationId);

            if (conversation == null)
                return NotFound("Conversation not found");

            // 🔥 SECURITY RULE: only participants can send messages
            var isParticipant =
                conversation.StudentId == userId ||
                conversation.AssignedAdminId == userId;

            if (!isParticipant)
                return Forbid("You are not part of this conversation");

            var message = new ChatMessage
            {
                ChatConversationId = dto.ConversationId,
                SenderId = userId,
                Message = dto.Message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);

            conversation.Status = ConversationStatus.Open;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                result = "Message sent",
                message.Id,
                message.Message,
                message.SentAt
            });
        }

        [Authorize]
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatConversationId == conversationId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        [Authorize]
        [HttpPut("read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);

            if (message == null)
                return NotFound();

            message.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Marked as read" });
        }
    }
}