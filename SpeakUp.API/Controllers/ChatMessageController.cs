using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Chat;
using SpeakUp.API.Models.ChatModel;
using SpeakUp.API.Models.UserModel;
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

            if (conversation.Status == ConversationStatus.Closed)
            {
                return BadRequest(new
                {
                    message = "This conversation has been closed by the administrator."
                });
            }

            _context.ChatMessages.Add(message);

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
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var conversation = await _context.ChatConversations
                .Include(c => c.Student)
                .FirstOrDefaultAsync(c => c.Id == conversationId);


            if (conversation == null)
                return NotFound("Conversation not found");


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
                    Id = m.Sender.Id,

                    Name =
        conversation.IsAnonymous &&
        m.Sender.Role == UserRole.Student
            ? "Anonymous User"
            : $"{m.Sender.FirstName} {m.Sender.LastName}",

                    Role = m.Sender.Role.ToString(),

                    IsCurrentUser = m.Sender.Id == userId
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

        [Authorize]
        [HttpPut("read/conversation/{conversationId}")]
        public async Task<IActionResult> MarkConversationAsRead(int conversationId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var messages = await _context.ChatMessages
                .Where(m =>
                    m.ChatConversationId == conversationId &&
                    m.SenderId != userId && 
                    !m.IsRead)
                .ToListAsync();

            foreach (var msg in messages)
            {
                msg.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Conversation marked as read",
                updated = messages.Count
            });
        }
    }
}