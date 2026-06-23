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
    public class ChatConversationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatConversationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE CONVERSATION
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateConversation(CreateConversationDto dto)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized();

            var userId = int.Parse(claim.Value);

            // 🔥 IF REPORT CHAT EXISTS, RETURN IT
            if (dto.ReportId != null)
            {
                var existingChat = await _context.ChatConversations
                    .FirstOrDefaultAsync(c =>
                        c.ReportId == dto.ReportId &&
                        c.StudentId == userId);

                if (existingChat != null)
                {
                    return Ok(new
                    {
                        message = "Existing conversation found",
                        id = existingChat.Id
                    });
                }
            }

            var conversation = new ChatConversation
            {
                ChatType = dto.ChatType,
                IsAnonymous = dto.IsAnonymous,
                StudentId = userId,
                ReportId = dto.ReportId,
                Status = ConversationStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.ReportId != null)
            {
                var report = await _context.Reports.FindAsync(dto.ReportId);

                if (report != null && report.AssignedAdminId != null)
                {
                    conversation.AssignedAdminId = report.AssignedAdminId;
                }
            }

            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Conversation created successfully",
                id = conversation.Id
            });
        }

        // USER: GET MY CONVERSATIONS (PAGINATED)
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyConversations(int page = 1, int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var query = _context.ChatConversations
                .Where(c => c.StudentId == userId || c.AssignedAdminId == userId);

            var totalCount = await query.CountAsync();

            var conversations = await query
                .Include(c => c.Messages)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Student)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = conversations.Select(c => new ConversationListDto
            {
                Id = c.Id,
                ChatType = c.ChatType.ToString(),
                Status = c.Status.ToString(),
                IsAnonymous = c.IsAnonymous,
                CreatedAt = c.CreatedAt,

                ReportId = c.ReportId,

                StudentId = c.StudentId,
                StudentName = c.Student != null
                    ? $"{c.Student.FirstName} {c.Student.LastName}"
                    : "Unknown",

                AssignedAdminId = c.AssignedAdminId,
                AssignedAdminName = c.AssignedAdmin != null
                    ? $"{c.AssignedAdmin.FirstName} {c.AssignedAdmin.LastName}"
                    : "Unassigned",

                LastMessage = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Message)
                    .FirstOrDefault(),

                LastMessageTime = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SentAt)
                    .FirstOrDefault()
            });

            return Ok(new { items = result, page, pageSize, totalCount });
        }

        // ADMIN: GET ALL CONVERSATIONS (PAGINATED)
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllConversations(int page = 1, int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var query = _context.ChatConversations.AsQueryable();

            if (!isSuperAdmin)
            {
                query = query.Where(c =>
                    c.AssignedAdminId == null || c.AssignedAdminId == userId
                );
            }

            var totalCount = await query.CountAsync();

            var conversations = await query
                .Include(c => c.Messages)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Student)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = conversations.Select(c => new ConversationListDto
            {
                Id = c.Id,
                ChatType = c.ChatType.ToString(),
                Status = c.Status.ToString(),
                IsAnonymous = c.IsAnonymous,
                CreatedAt = c.CreatedAt,
                ReportId = c.ReportId,

                StudentId = c.StudentId,
                StudentName = c.Student != null
                    ? $"{c.Student.FirstName} {c.Student.LastName}"
                    : "Unknown",

                AssignedAdminId = c.AssignedAdminId,
                AssignedAdminName = c.AssignedAdmin != null
                    ? $"{c.AssignedAdmin.FirstName} {c.AssignedAdmin.LastName}"
                    : "Unassigned",

                LastMessage = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Message)
                    .FirstOrDefault(),

                LastMessageTime = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SentAt)
                    .FirstOrDefault()
            });

            return Ok(new { items = result, page, pageSize, totalCount });
        }

        // ADMIN: GET UNASSIGNED (QUEUE SYSTEM)
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/unassigned")]
        public async Task<IActionResult> GetUnassigned()
        {
            var conversations = await _context.ChatConversations
                .Where(c => c.AssignedAdminId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(conversations);
        }

        // ADMIN: GET MY ASSIGNED CHATS
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/assigned-to-me")]
        public async Task<IActionResult> GetMyAssignedChats()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized();

            var adminId = int.Parse(claim.Value);

            var conversations = await _context.ChatConversations
                .Where(c => c.AssignedAdminId == adminId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(conversations);
        }

        // ADMIN: GET CLOSED CHATS
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/closed")]
        public async Task<IActionResult> GetClosedChats()
        {
            var conversations = await _context.ChatConversations
                .Where(c => c.Status == ConversationStatus.Closed)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(conversations);
        }

        // ADMIN: ASSIGN ADMIN
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpPut("assign")]
        public async Task<IActionResult> AssignAdmin(AssignAdminDto dto)
        {
            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == dto.ConversationId);

            if (conversation == null)
                return NotFound("Conversation not found");

            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.AdminId);

            if (admin == null)
                return NotFound("Admin not found");

            if (admin.Role != UserRole.JuniorAdmin && admin.Role != UserRole.SuperAdmin)
                return BadRequest("User is not an admin");

            if (conversation.AssignedAdminId == dto.AdminId)
                return BadRequest("Already assigned to this admin");

            conversation.PreviousAdminId = conversation.AssignedAdminId;
            conversation.AssignedAdminId = dto.AdminId;
            conversation.Status = ConversationStatus.Open;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Admin assigned successfully",
                conversation.Id,
                conversation.AssignedAdminId
            });
        }

        [Authorize]
        [HttpGet("by-report/{reportId}")]
        public async Task<IActionResult> GetChatByReport(int reportId)
        {
            var conversation = await _context.ChatConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ReportId == reportId);

            if (conversation == null)
                return NotFound("No chat found for this report");

            var result = new
            {
                conversation.Id,
                conversation.ChatType,
                conversation.Status,
                conversation.IsAnonymous,
                conversation.CreatedAt,
                conversation.ClosedAt,

                Messages = conversation.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        m.Id,
                        m.Message,
                        m.SenderId,
                        m.SentAt,
                        m.IsRead
                    })
            };

            return Ok(result);
        }

        // ADMIN: CLOSE CONVERSATION
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpPut("close/{conversationId}")]
        public async Task<IActionResult> CloseConversation(int conversationId)
        {
            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return NotFound("Conversation not found");

            if (conversation.Status == ConversationStatus.Closed)
                return BadRequest("Conversation is already closed");

            conversation.Status = ConversationStatus.Closed;
            conversation.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Conversation closed successfully",
                conversation.Id,
                conversation.Status,
                conversation.ClosedAt
            });
        }
    }
}