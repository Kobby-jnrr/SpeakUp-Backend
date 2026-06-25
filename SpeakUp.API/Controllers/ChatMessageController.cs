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

            var isParticipant =
                conversation.StudentId == userId ||
                conversation.AssignedAdminId == userId;

            if (!isParticipant)
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "Forbidden",
                    message = "You are not part of this conversation."
                });

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
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.ChatConversationId,
                    m.Message,
                    m.SentAt,
                    m.IsRead,

                    Sender = new
                    {
                        m.Sender.Id,
                        m.Sender.FirstName,
                        m.Sender.LastName,
                        m.Sender.Role
                    }
                })
                .ToListAsync();

            return Ok(messages);
        }


        [Authorize]
        [HttpPut("read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return NotFound();

            message.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Marked as read"
            });
        }
    }
}