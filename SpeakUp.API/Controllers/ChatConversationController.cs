using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Chat;
using SpeakUp.API.Models.ChatModel;
using SpeakUp.API.Models.UserModel;
using SpeakUp.API.Services;
using System.Security.Claims;

namespace SpeakUp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatConversationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;


        public ChatConversationController(
            ApplicationDbContext context,
            AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }


        // CREATE CONVERSATION
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateConversation(CreateConversationDto dto)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


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

                if (report?.AssignedAdminId != null)
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


        // USER: MY CONVERSATIONS
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyConversations(
            int page = 1,
            int pageSize = 10)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var query = _context.ChatConversations
                .Where(c =>
                    c.StudentId == userId ||
                    c.AssignedAdminId == userId);



            var totalCount = await query.CountAsync();



            var conversations = await query
                .Include(c => c.Student)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages)
                .Include(c => c.Report)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();



            var result = conversations
                .Select(c => MapConversation(c, userId));



            return Ok(new
            {
                items = result,
                page,
                pageSize,
                totalCount
            });
        }


        // ADMIN: ALL CONVERSATIONS
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllConversations(
            int page = 1,
            int pageSize = 10)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var isSuperAdmin = User.IsInRole("SuperAdmin");


            var query = _context.ChatConversations.AsQueryable();



            if (!isSuperAdmin)
            {
                query = query.Where(c =>
                    c.AssignedAdminId == null ||
                    c.AssignedAdminId == userId);
            }



            var totalCount = await query.CountAsync();



            var conversations = await query
                .Include(c => c.Student)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages)
                .Include(c => c.Report)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();



            var result = conversations
                .Select(c => MapConversation(c, userId));



            return Ok(new
            {
                items = result,
                page,
                pageSize,
                totalCount
            });
        }


        // ADMIN: UNASSIGNED
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/unassigned")]
        public async Task<IActionResult> GetUnassigned()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var conversations = await _context.ChatConversations
                .Where(c => c.AssignedAdminId == null)
                .Include(c => c.Student)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages)
                .Include(c => c.Report)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();



            var result = conversations
                .Select(c => MapConversation(c, userId));



            return Ok(new
            {
                items = result
            });
        }


        // ADMIN: ASSIGNED TO ME
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/assigned-to-me")]
        public async Task<IActionResult> GetMyAssignedChats()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var conversations = await _context.ChatConversations
                .Where(c => c.AssignedAdminId == userId)
                .Include(c => c.Student)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages)
                .Include(c => c.Report)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();



            var result = conversations
                .Select(c => MapConversation(c, userId));



            return Ok(new
            {
                items = result
            });
        }


        // ADMIN: CLOSED
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpGet("admin/closed")]
        public async Task<IActionResult> GetClosedChats()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var conversations = await _context.ChatConversations
                .Where(c => c.Status == ConversationStatus.Closed)
                .Include(c => c.Student)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Messages)
                .Include(c => c.Report)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();



            var result = conversations
                .Select(c => MapConversation(c, userId));



            return Ok(new
            {
                items = result
            });
        }


        // ASSIGN ADMIN
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpPut("assign")]
        public async Task<IActionResult> AssignAdmin(
            AssignAdminDto dto)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var conversation = await _context.ChatConversations
                .Include(c => c.Student)
                .Include(c => c.AssignedAdmin)
                .FirstOrDefaultAsync(c => c.Id == dto.ConversationId);

            if (conversation == null)
                return NotFound("Conversation not found");

            var admin = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.AdminId);

            if (admin == null)
                return NotFound("Admin not found");



            if (admin.Role != UserRole.JuniorAdmin &&
                admin.Role != UserRole.SuperAdmin)
            {
                return BadRequest("Invalid admin role");
            }

            conversation.PreviousAdminId =
                conversation.AssignedAdminId;


            conversation.AssignedAdminId =
                dto.AdminId;

            conversation.Status =
                ConversationStatus.Open;

            await _context.SaveChangesAsync();

            var studentName = conversation.IsAnonymous
            ? "Anonymous User"
            : conversation.Student != null
                ? $"{conversation.Student.FirstName} {conversation.Student.LastName}"
                : "Unknown Student";

            await _auditService.Log(
                userId,
                "Claimed Chat",
                $"{studentName}'s conversation was assigned to {admin.FirstName} {admin.LastName}."
            );



            return Ok(new
            {
                message = "Admin assigned successfully",
                conversation.Id,
                conversation.AssignedAdminId
            });
        }


        // BY REPORT
        [Authorize]
        [HttpGet("by-report/{reportId}")]
        public async Task<IActionResult> GetChatByReport(int reportId)
        {
            var conversation = await _context.ChatConversations
                .Include(c => c.Messages)
                .Include(c => c.Student)
                .Include(c => c.AssignedAdmin)
                .Include(c => c.Report)
                .FirstOrDefaultAsync(c =>
                    c.ReportId == reportId);



            if (conversation == null)
                return NotFound("No chat found");



            return Ok(new
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
            });
        }


        // CLOSE CHAT
        [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
        [HttpPut("close/{conversationId}")]
        public async Task<IActionResult> CloseConversation(
            int conversationId)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );


            var conversation = await _context.ChatConversations
                .Include(c => c.Student)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return NotFound("Conversation not found");

            if (conversation.Status == ConversationStatus.Closed)
                return BadRequest("Already closed");

            if (conversation.AssignedAdminId == null)
                return BadRequest("Conversation has not been claimed yet");

            if (conversation.AssignedAdminId != userId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Only assigned admin can close this conversation"
                );
            }


            conversation.Status = ConversationStatus.Closed;
            conversation.ClosedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();


            var studentName = conversation.IsAnonymous
            ? "Anonymous User"
            : conversation.Student != null
                ? $"{conversation.Student.FirstName} {conversation.Student.LastName}"
                : "Unknown Student";

            await _auditService.Log(
                userId,
                "Closed Chat",
                $"{studentName}'s conversation was closed."
            );


            return Ok(new
            {
                message = "Conversation closed successfully",
                conversation.Id,
                conversation.Status,
                conversation.ClosedAt
            });
        }


        private object MapConversation(
            ChatConversation c,
            int userId)
        {
            return new ConversationListDto
            {
                Id = c.Id,

                ChatType = c.ChatType.ToString(),

                Status = c.Status.ToString(),

                IsAnonymous = c.IsAnonymous,

                CreatedAt = c.CreatedAt,

                ReportId = c.ReportId,

                ReportCode = c.Report != null
                    ? $"REP-{c.Report.Id.ToString().PadLeft(6, '0')}"
                    : null,


                StudentId = c.StudentId,

                StudentName = c.IsAnonymous
                    ? "Anonymous User"
                    : c.Student != null
                        ? $"{c.Student.FirstName} {c.Student.LastName}"
                        : "Unknown Student",

                AssignedAdminId = c.AssignedAdminId,

                AssignedAdminName = c.AssignedAdmin != null
                    ? $"{c.AssignedAdmin.FirstName} {c.AssignedAdmin.LastName}"
                    : "Unassigned",

                LastMessage = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Message)
                    .FirstOrDefault(),

                UnreadCount = c.Messages.Count(m =>
                    !m.IsRead &&
                    m.SenderId != userId),

                LastMessageTime = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SentAt)
                    .FirstOrDefault()
            };
        }
    }
}